using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

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

		var startLoc = reader.Consumed;
		SkipWhitespaceAndComments(ref reader);

		if (reader.End || !reader.TryPeek(out byte b))
			return Result.Success(new Token(TokenKind.Eof, startLoc, startLoc));

		if (!b.IsAscii())
		{
			return Result.Failure<Token>(new(
				LexerErrorCodes.InvalidData,
				startLoc, currentPosition,
				startLoc, currentPosition,
				"Expected an ASCII character but received a non-ASCII character.",
				Severity.Error
			));
		}

		char c = (char)b;

		// strings
		if (c == '"')
			return ScanStringLiteral(ref reader, startLoc, ref currentPosition);

		// number literals
		if (b.IsAsciiDigit())
			return ScanNumber(ref reader, startChar: b, startLoc, ref currentPosition);

		// identifiers and keywords
		if (b.IsAsciiLetter() || c == '_')
			return ScanIdentifierOrKeyword(ref reader, startLoc);

		// standard library identifiers
		if (c == '$')
			return ScanStdIdentifier(ref reader, startLoc);

		// member access notation
		if (c == '.')
			return Result.Success(new Token(TokenKind.MemberAccess, startLoc, 1L));

		// punctuation
		if (b.IsRsmlPunctuation())
			return ScanPunctuation(ref reader, startLoc);

		// todo: check for comments if Configuration.EmitComments is enabled

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

		while (Configuration.MaximumAllowedFailuresPerComponent <= 0 || failedRuns < Configuration.MaximumAllowedFailuresPerComponent)
		{
			// todo: create reader here
			var token = GetNextToken(ref reader);

			if (token.IsError)
			{
				Diagnostics.Add(token.Error);
				failedRuns++;
				continue;
			}

			if (token.Value.Kind == TokenKind.Eof)
				yield break;

			else
				yield return token.Value;
		}

		throw new ExceededMaxAmountOfFailuresException(
			$"This instance of the lexer was allowed to fail up to {Configuration.MaximumAllowedFailuresPerComponent} times, yet it failed {failedRuns}."
		);
	}

	private static Result<Token> ScanNumber(ref SequenceReader<byte> reader, byte startChar, long startLoc, ref AbsolutePosition position)
	{
		var startPos = position;
		const byte underscore = (byte)'_';
		const byte dot = (byte)'.';

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

			if (b == dot)
				hasDotSeparator = true;

		} while (!reader.End && reader.TryPeek(out b) && (b.IsAsciiDigit() || b == underscore || (b == dot && !hasDotSeparator)));

		return Result.Success(new Token(TokenKind.NumericLiteral, startLoc, reader.Consumed - startLoc));
	}

	private static Result<Token> ScanStringLiteral(ref SequenceReader<byte> reader, long startLoc, ref AbsolutePosition position)
	{
		var startPos = position;
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

	// todo: fix the method below
	private Result<Token> ScanStdIdentifier(ref SequenceReader<byte> reader, long startLoc)
	{
		// this points to h in $helloWorld broski
		int afterStdSymbolIndex = ++cursor; // we also skip past it to avoid extra checks in while loop

		while (cursor < Sequence.Length && (Char.IsAsciiLetterOrDigit(Sequence[cursor]) || Sequence[cursor] == '_'))
			cursor++;

		return cursor == afterStdSymbolIndex
			? Result.Failure<Token>(new(
				LexerErrorCodes.ExpectedStdIdentifier,
				Sequence.GetLocationDetails((Index)startLoc),
				Sequence.GetLocationDetails((Index)cursor),
				"Expected a standard library identifier, yet there was no valid identifier after the $ symbol.",
				Severity.Error
			))
			: Result.Success(new Token(TokenKind.StandardLibraryIdentifier, null, startLoc..cursor));
	}

	// todo: fix the method below
	private Result<Token> ScanIdentifierOrKeyword(ref SequenceReader<byte> reader, long startLoc)
	{
		while (cursor < Sequence.Length && (Char.IsAsciiLetterOrDigit(Sequence[cursor]) || Sequence[cursor] == '_'))
			cursor++;

		Range range = startLoc..cursor;

		if (Keywords.Contains(Sequence[range]))
		{
			var token = new Token(Token.GetKeywordKind(Sequence[range]), null, range); // is keyword

			return token.Kind == TokenKind.Unknown
				? Result.Failure<Token>(new(
					LexerErrorCodes.FailedToIdentifyKeyword,
					Sequence.GetLocationDetails(startLoc),
					Sequence.GetLocationDetails(cursor),
					"Despite identifying the object in question as a keyword, the lexer failed to resolve exactly which keyword it was." +
					"This likely means the keyword in question is reserved for future use, but isn't implemented yet.",
					Severity.Error
				))
				: Result.Success(token);
		}
		else
		{
			return Result.Success(new Token(TokenKind.Identifier, null, range)); // is identifier
		}
	}

	// todo: fix the method below
	private Result<Token> ScanPunctuation(ref SequenceReader<byte> reader, long startLoc)
	{
		char c = Sequence[cursor];
		char? peeked = cursor + 1 >= Sequence.Length ? null : Sequence[++cursor]; // dont error out if out of bounds
		TokenKind kind = Token.GetPunctuationKind(c, peeked);

		return kind == TokenKind.Unknown
			? Result.Failure<Token>(new(
				LexerErrorCodes.FailedToIdentifyPunctuation,
				Sequence.GetLocationDetails(startLoc),
				Sequence.GetLocationDetails(peeked is null ? cursor - 1 : cursor),
				"Despite identifying the object in question as punctuation, the lexer failed to resolve exactly which punctuation it was." +
				"This might mean the punctuation in question is reserved for future use, and not implemented yet.",
				Severity.Error
			))
			: Result.Success(new Token(kind, null, startLoc..cursor));
	}

	private void SkipWhitespaceAndComments(ref SequenceReader<byte> reader)
	{
		while (!reader.End && reader.TryPeek(out byte b))
		{
			if (b.IsAsciiWhitespace())
				reader.Advance(1);

			else if (b == (byte)'#' && !Configuration.EmitComments)
				reader.TryAdvanceToAny([(byte)'\r', (byte)'\n'], advancePastDelimiter: true);

			else
				break;
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

	/// <summary>
	/// Checks if the given position matches the cursor position in the
	/// reader. This does not check if the line and column numbers are correct.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="position">The expected current position.</param>
	/// <returns>True if the given position matches the reader's cursor position, in offset.</returns>
	public static bool IsCorrectPosition(in SequenceReader<byte> reader, AbsolutePosition position) => position.Offset == reader.Consumed;

	// todo: implement the method below
	public Task<IEnumerable<Token>> LexAsync(PipeStream stream, CancellationToken? cancellationToken = null) => throw new NotImplementedException();

	// todo: implement the method below
	public IEnumerable<Token> Lex(string? data) => throw new NotImplementedException();

	// todo: implement the method below
	public IEnumerable<Token> Lex(char[] data) => throw new NotImplementedException();
}
