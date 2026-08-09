using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using OceanApocalypse.RSML.Language.Lexing.Diagnostics;
using OceanApocalypse.RSML.Language.Lexing.Tokens;
using OceanApocalypse.RSML.Abstractions;
using OceanApocalypse.RSML.Abstractions.Diagnostics;
using OceanApocalypse.RSML.Abstractions.Sources;
using OceanApocalypse.RSML.Abstractions.Panic;

namespace OceanApocalypse.RSML.Language.Lexing;

/// <summary>
/// An implementation of a RSML lexer backed by a read-only or read-and-write buffer.
/// </summary>
/// <param name="buffer">A buffer. Can be read-only (<see cref="IBuffer"/>) or read and write (<see cref="IBuffer"/>).</param>
/// <param name="diagnostics">A collector for all emitted diagnostics.</param>
public class BufferLexer(IBuffer buffer, DiagnosticCollector diagnostics) : Lexer
{
	private int cursor;

	/// <remarks>
	/// :::note[Diagnostic output]
	/// This method does not add diagnostics to the collector
	/// (<see cref="DiagnosticCollector"/>): it only returns them when it
	/// proves necessary.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public override Result<Token> GetNextToken()
	{
		SkipWhitespaceAndComments();

		if (cursor >= buffer.Length)
			return Result.Success(new Token(TokenKind.Eof, null, new()));

		int startLoc = cursor;
		char c = buffer[cursor];

		// strings
		if (c == '"')
			return ScanStringLiteral(startLoc);

		// number literals
		if (Char.IsAsciiDigit(c))
			return ScanNumber(startLoc);

		// identifiers and keywords
		if (Char.IsAsciiLetter(c) || c == '_')
			return ScanIdentifierOrKeyword(startLoc);

		// standard library identifiers
		if (c == '$')
			return ScanStdIdentifier(startLoc);

		// member access notation
		if (c == '.')
			return Result.Success(new Token(TokenKind.MemberAccess, null, new(startLoc, ++cursor)));

		// punctuation
		if (c.IsAsciiPunctuation())
			return ScanPunctuation(startLoc);

		return Result.Failure<Token>(new(
			LexerErrorCodes.FailedToLexToken,
			"Tried all possible token logic paths, but none was true. This likely means you used a character not recognized by the lexer," +
			"but it may also mean the lexer is mal-functioning.",
			Severity.Critical
		));
	}

	/// <inheritdoc/>
	public override IEnumerable<Token> Lex()
	{
		// todo: make these customizable configurations
		int maxFailedRunsLimit = 10;
		int failedRuns = 0;

		while (failedRuns < maxFailedRunsLimit)
		{
			var token = GetNextToken();

			if (token.IsError)
			{
				diagnostics.Add(token.Error);
				failedRuns++;
				continue;
			}

			if (token.Value.Kind == TokenKind.Eof)
				yield break;

			else
				yield return token.Value;
		}

		throw new ExceededMaxAmountOfFailuresException(
			$"This instance of the lexer was allowed to fail up to {maxFailedRunsLimit} times, yet it failed {failedRuns}."
		);
	}

	private Result<Token> ScanNumber(int startLoc)
	{
		bool dot = false;

		while (cursor < buffer.Length && (Char.IsAsciiDigit(buffer[cursor]) || buffer[cursor] == '_' || buffer[cursor] == '.'))
		{
			if (buffer[cursor] == '.')
			{
				if (dot)
					return Result.Success(new Token(TokenKind.NumericLiteral, null, new(startLoc, cursor)));

				else
					dot = true;
			}

			cursor++;
		}

		return Result.Success(new Token(TokenKind.NumericLiteral, null, new(startLoc, cursor)));
	}

	private Result<Token> ScanStringLiteral(int startLoc)
	{
		cursor++;
		bool escaping = false;

		while (cursor < buffer.Length)
		{
			if (buffer[cursor].IsNewline())
			{
				return Result.Failure<Token>(new(
					LexerErrorCodes.UnterminatedStringLiteral,
					buffer.GetLocationDetails((Index)startLoc),
					buffer.GetLocationDetails((Index)cursor),
					"A string literal must begin and end in the same line.",
					Severity.Error
				));
			}

			if (buffer[cursor] == '"' && !escaping)
				break;

			if (buffer[cursor] == '\\')
				escaping = !escaping;

			cursor++;
		}

		if (cursor < buffer.Length)
			cursor++; // skip end quote if anything beyond it

		return Result.Success(new Token(TokenKind.StringLiteral, null, startLoc..cursor));
	}

	private Result<Token> ScanStdIdentifier(int startLoc)
	{
		// this points to h in $helloWorld broski
		int afterStdSymbolIndex = ++cursor; // we also skip past it to avoid extra checks in while loop

		while (cursor < buffer.Length && (Char.IsAsciiLetterOrDigit(buffer[cursor]) || buffer[cursor] == '_'))
			cursor++;

		return cursor == afterStdSymbolIndex
			? Result.Failure<Token>(new(
				LexerErrorCodes.ExpectedStdIdentifier,
				buffer.GetLocationDetails((Index)startLoc),
				buffer.GetLocationDetails((Index)cursor),
				"Expected a standard library identifier, yet there was no valid identifier after the $ symbol.",
				Severity.Error
			))
			: Result.Success(new Token(TokenKind.StandardLibraryIdentifier, null, startLoc..cursor));
	}

	private Result<Token> ScanIdentifierOrKeyword(int startLoc)
	{
		while (cursor < buffer.Length && (Char.IsAsciiLetterOrDigit(buffer[cursor]) || buffer[cursor] == '_'))
			cursor++;

		Range range = startLoc..cursor;

		if (Keywords.Contains(buffer[range]))
		{
			var token = new Token(Token.GetKeywordKind(buffer[range]), null, range); // is keyword

			return token.Kind == TokenKind.Unknown
				? Result.Failure<Token>(new(
					LexerErrorCodes.FailedToIdentifyKeyword,
					buffer.GetLocationDetails(startLoc),
					buffer.GetLocationDetails(cursor),
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

	private Result<Token> ScanPunctuation(int startLoc)
	{
		char c = buffer[cursor];
		char? peeked = cursor + 1 >= buffer.Length ? null : buffer[++cursor]; // dont error out if out of bounds
		TokenKind kind = Token.GetPunctuationKind(c, peeked);

		return kind == TokenKind.Unknown
			? Result.Failure<Token>(new(
				LexerErrorCodes.FailedToIdentifyPunctuation,
				buffer.GetLocationDetails(startLoc),
				buffer.GetLocationDetails(peeked is null ? cursor - 1 : cursor),
				"Despite identifying the object in question as punctuation, the lexer failed to resolve exactly which punctuation it was." +
				"This might mean the punctuation in question is reserved for future use, and not implemented yet.",
				Severity.Error
			))
			: Result.Success(new Token(kind, null, startLoc..cursor));
	}

	private void SkipWhitespaceAndComments()
	{
		while (cursor < buffer.Length)
		{
			char c = buffer[cursor];

			if (Char.IsWhiteSpace(c))
			{
				cursor += buffer.CountUntilNotWhitespace(cursor);
			}
			else if (c == '#')
			{
				while (!buffer[cursor].IsNewline())
					cursor++;
			}
			else
			{
				break;
			}
		}
	}
}
