using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Text;

using OceanApocalypse.RSML.Abstractions;
using OceanApocalypse.RSML.Abstractions.Diagnostics;
using OceanApocalypse.RSML.Abstractions.Panic;
using OceanApocalypse.RSML.Abstractions.Toolchain;
using OceanApocalypse.RSML.Language.Lexing.Diagnostics;
using OceanApocalypse.RSML.Language.Lexing.Tokens;

namespace OceanApocalypse.RSML.Language.Lexing.Utf8;

/// <summary>
/// An implementation of a RSML lexer backed by a given UTF-8 buffer.
/// </summary>
/// <remarks>
/// Initializes a new lexer with a given configuration and diagnostic collector.
/// </remarks>
/// <param name="diagnosticCollector">A collector with all the diagnostics that were and will be emitted.</param>
/// <param name="configuration">Configurations for the toolchain components.</param>
public class Utf8Lexer(DiagnosticCollector diagnosticCollector, ToolchainConfiguration? configuration = null) : IUtf8Lexer<byte>
{
	private bool isDisposed;
	private bool wasUsed;

	/// <summary>
	/// A collector containing all emitted diagnostics.
	/// </summary>
	protected DiagnosticCollector Diagnostics { get; } = diagnosticCollector;

	/// <inheritdoc/>
	public ToolchainConfiguration Configuration { get; protected set; } = configuration ?? ToolchainConfiguration.Default;

	/// <remarks>
	/// :::note[Diagnostic output]
	/// This method does not add diagnostics to the collector
	/// (<see cref="DiagnosticCollector"/>): it only returns them when it
	/// proves necessary.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public Result<Token> GetNextToken(ref SequenceReader<byte> reader, ref AbsolutePosition currentPosition)
	{
		wasUsed = true;
		AbsolutePosition.ThrowIfInvalid(currentPosition);

		SkipWhitespaceAndComments(ref reader, ref currentPosition);

		if (reader.End || !reader.TryPeek(out byte b))
			return Result.Success(new Token(TokenKind.Eof, reader.Consumed, reader.Consumed));

		if (!b.IsAscii())
		{
			return Result.Failure<Token>(new(
				LexerErrorCodes.InvalidData,
				reader.Consumed, currentPosition,
				reader.Consumed, currentPosition,
				"Expected an ASCII character but received a non-ASCII character.",
				Severity.Error
			));
		}

		char c = (char)b;

		// strings
		if (c == '"')
			return ScanStringLiteral(ref reader, ref currentPosition);

		// number literals
		if (b.IsAsciiDigit())
			return ScanNumber(ref reader, startChar: b, ref currentPosition);

		// identifiers and keywords
		if (b.IsAsciiLetter() || c == '_')
			return ScanIdentifierOrKeyword(ref reader, ref currentPosition);

		// standard library identifiers
		if (c == '$')
			return ScanStdIdentifier(ref reader, ref currentPosition);

		// member access notation
		if (c == '.')
		{
			reader.Advance(1);
			currentPosition.Column++;
			return Result.Success(new Token(TokenKind.MemberAccess, reader.Consumed, 1));
		}

		// punctuation
		if (b.IsRsmlPunctuation())
			return ScanPunctuation(ref reader, ref currentPosition);

		// comments
		if (Configuration.EmitComments && b == (byte)'#')
			return ScanComment(ref reader, ref currentPosition);

		return Result.Failure<Token>(new(
			code: LexerErrorCodes.FailedToLexToken,
			"Tried all possible token logic paths, but none was true. This likely means you used a character not recognized by the lexer," +
			"but it may also mean the lexer is mal-functioning.",
			Severity.Critical
		));
	}

	/// <inheritdoc/>
	public IEnumerable<Token> Lex(ReadOnlySequence<byte> data)
	{
		wasUsed = true;
		int failedRuns = 0;

		var reader = new SequenceReader<byte>(data);
		var position = AbsolutePosition.Default;
		var writer = new ArrayBufferWriter<Token>((int)(data.Length / 2));

		if (!position.IsValid)
		{
			position.Line = 1;
			position.Column = 1;
		}

		while (Configuration.MaximumAllowedFailuresPerComponent <= 0 || failedRuns < Configuration.MaximumAllowedFailuresPerComponent)
		{
			Span<Token> tokens = writer.GetSpan(64);
			int idx = 0;

			while (idx < tokens.Length && Configuration.MaximumAllowedFailuresPerComponent <= 0 || failedRuns < Configuration.MaximumAllowedFailuresPerComponent)
			{
				var token = GetNextToken(ref reader, ref position);

				if (token.IsError)
				{
					Diagnostics.Add(token.Error);
					failedRuns++;
					continue;
				}

				if (token.Value.Kind == TokenKind.Eof)
				{
					return writer.WrittenSpan.ToArray();
				}

				else
				{
					tokens[idx] = token.Value;
					idx++;
				}
			}

			writer.Advance(idx);
		}

		throw new ExceededMaxAmountOfFailuresException(
			$"This instance of the lexer was allowed to fail up to {Configuration.MaximumAllowedFailuresPerComponent} times, yet it failed {failedRuns}."
		);
	}

	private static Result<Token> ScanNumber(ref SequenceReader<byte> reader, byte startChar, ref AbsolutePosition position)
	{
		var startPos = position;
		var startLoc = reader.Consumed;

		byte b = startChar;
		bool hasDotSeparator = false;

		do
		{
			reader.Advance(1);
			position.Column++;

			if (!b.IsAscii())
			{
				return Result.Failure<Token>(new(
					LexerErrorCodes.InvalidData,
					startLoc, startPos,
					startLoc, position,
					"Expected an ASCII character but received a non-ASCII character.",
					Severity.Error
				));
			}

			if (b == '.')
				hasDotSeparator = true;

		} while (!reader.End && reader.TryPeek(out b) && (b.IsAsciiDigit() || b == '_' || (b == '.' && !hasDotSeparator)));

		return Result.Success(new Token(TokenKind.NumericLiteral, startLoc, reader.Consumed - startLoc));
	}

	private static Result<Token> ScanStringLiteral(ref SequenceReader<byte> reader, ref AbsolutePosition position)
	{
		var startPos = position;
		var startLoc = reader.Consumed;
		bool escaping = false;

		if (reader.End)
		{
			return Result.Failure<Token>(new(
				LexerErrorCodes.UnterminatedStringLiteral,
				startLoc, startPos,
				reader.Consumed, position,
				"A string literal must begin and end in the same line.",
				Severity.Error
			));
		}

		reader.Advance(1);
		position.Column++;

		while (!reader.End && reader.TryPeek(out byte b))
		{
			reader.Advance(1);
			position.Column++;

			if (b.IsAsciiNewline())
			{
				position.MoveToStartOfNextLine();

				return Result.Failure<Token>(new(
					LexerErrorCodes.UnterminatedStringLiteral,
					startLoc, startPos,
					reader.Consumed, position,
					"A string literal must begin and end in the same line.",
					Severity.Error
				));
			}

			if (b == '"' && !escaping)
				break;

			if (b == '\\')
				escaping = !escaping;
		}

		return Result.Success(new Token(TokenKind.StringLiteral, startLoc, reader.Consumed - startLoc));
	}

	private static Result<Token> ScanStdIdentifier(ref SequenceReader<byte> reader, ref AbsolutePosition position)
	{
		var startPos = position;
		var startLoc = reader.Consumed;

		do
		{
			reader.Advance(1);
			position.Column++;

		} while (!reader.End && reader.TryPeek(out byte b) && (b.IsAsciiLetter() || b.IsAsciiDigit() || b == '_'));

		return reader.Consumed == startLoc + 1
			? Result.Failure<Token>(new(
				LexerErrorCodes.ExpectedStdIdentifier,
				startLoc, startPos,
				reader.Consumed, position,
				"Expected a standard library identifier, yet there was no valid identifier after the $ symbol.",
				Severity.Error
			))
			: Result.Success(new Token(TokenKind.StandardLibraryIdentifier, startLoc, reader.Consumed - startLoc));
	}

	private static Result<Token> ScanIdentifierOrKeyword(ref SequenceReader<byte> reader, ref AbsolutePosition position)
	{
		var startPos = position;
		var startLoc = reader.Consumed;

		while (!reader.End && reader.TryPeek(out byte b) && (b.IsAsciiLetter() || b.IsAsciiDigit() || b == '_'))
		{
			reader.Advance(1);
			position.Column++;
		}

		int sliceLength = (int)(reader.Consumed - startLoc);
		var slice = reader.Sequence.Slice(startLoc, sliceLength);

		if (Token.Keywords.ContainsUtf8(slice))
		{
			var token = new Token(Token.GetKeywordKind(slice), startLoc, sliceLength); // is keyword

			return token.Kind == TokenKind.Unknown
				? Result.Failure<Token>(new(
					LexerErrorCodes.FailedToIdentifyKeyword,
					startLoc, startPos,
					reader.Consumed, position,
					"Despite identifying the object in question as a keyword, the lexer failed to resolve exactly which keyword it was." +
					"This likely means the keyword in question is reserved for future use, but isn't implemented yet.",
					Severity.Error
				))
				: Result.Success(token);
		}
		else
		{
			return Result.Success(new Token(TokenKind.Identifier, startLoc, sliceLength)); // is identifier
		}
	}

	private static Result<Token> ScanPunctuation(ref SequenceReader<byte> reader, ref AbsolutePosition position)
	{
		var startPos = position;
		var startLoc = reader.Consumed;

		reader.TryRead(out byte first); // will always work and will always be ASCII
		position.Column++;

		var successful = reader.TryPeek(out byte second);

		if (successful && !second.IsAscii())
			successful = false;

		if (successful)
		{
			reader.Advance(1);
			position.Column++;
		}

		TokenKind kind = Token.GetPunctuationKind(first, successful ? second : (byte)0);

		return kind == TokenKind.Unknown
			? Result.Failure<Token>(new(
				LexerErrorCodes.FailedToIdentifyPunctuation,
				startLoc, startPos,
				reader.Consumed, position,
				"Despite identifying the object in question as punctuation, the lexer failed to resolve exactly which punctuation it was." +
				"This might mean the punctuation in question is reserved for future use, and not implemented yet.",
				Severity.Error
			))
			: Result.Success(new Token(kind, startLoc, reader.Consumed - startLoc));
	}

	private static Result<Token> ScanComment(ref SequenceReader<byte> reader, ref AbsolutePosition position)
	{
		var startLoc = reader.Consumed;
		var found = reader.TryAdvanceToAny([(byte)'\r', (byte)'\n'], advancePastDelimiter: false);
		// not advancing past cuz it gets handled in next GetNextToken call

		if (!found) // consume everything - we're EOF
			reader.AdvanceToEnd();

		int tokenLength = (int)(reader.Consumed - startLoc);
		position.Column += tokenLength;
		return Result.Success(new Token(TokenKind.Comment, startLoc, tokenLength));
	}

	private void SkipWhitespaceAndComments(ref SequenceReader<byte> reader, ref AbsolutePosition position)
	{
		while (!reader.End && reader.TryPeek(out byte b))
		{
			if (b.IsAsciiNewline())
			{
				reader.Advance(1);
				position.MoveToStartOfNextLine();

				if (b == (byte)'\r' && reader.TryPeek(out byte next) && next == (byte)'\n')
					reader.Advance(1);
			}
			else if (b.IsAsciiWhitespace())
			{
				position.Column += (int)reader.AdvancePastAny([9, 11, 12, 30]);
			}
			else if (b == (byte)'#' && !Configuration.EmitComments)
			{
				_ = ScanComment(ref reader, ref position);
			}
			else
			{
				break;
			}
		}
	}

	/// <inheritdoc/>
	public void Inject(ToolchainConfiguration configuration)
	{
		if (wasUsed)
			throw new ReadOnlyException("The configuration has already been apply and cannot be altered.");

		Configuration = configuration;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes of internally used unmanaged resources.
	/// </summary>
	/// <param name="disposing">If true, also disposes of managed resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
			return;

		// unmanaged things here

		if (disposing)
		{ } // managed things

		isDisposed = true;
	}

	/// <inheritdoc/>
	public IEnumerable<Token> Lex(string? data)
	{
		ArgumentException.ThrowIfNullOrEmpty(data);
		return Lex(new ReadOnlySequence<byte>(Encoding.Default.GetBytes(data)));
	}

	/// <inheritdoc/>
	public IEnumerable<Token> Lex(char[] data)
	{
		ArgumentNullException.ThrowIfNull(data);
		return Lex(new ReadOnlySequence<byte>(Encoding.Default.GetBytes(data)));
	}
}
