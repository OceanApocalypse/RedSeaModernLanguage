using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using OceanApocalypse.RSML.Language.Lexing.Diagnostics;
using OceanApocalypse.RSML.Language.Lexing.Tokens;
using OceanApocalypse.RSML.Toolchain.Abstractions;
using OceanApocalypse.RSML.Toolchain.Abstractions.Diagnostics;
using OceanApocalypse.RSML.Toolchain.Abstractions.Sources;

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

		if (c == '.')
			return Result.Success(new Token(TokenKind.MemberAccess, null, new(startLoc, ++cursor)));

		// todo: add the remaining possible paths
		return Result.Failure<Token>(new(LexerErrorCodes.FailedToLexToken, "Tried all possible token logic paths, but none was true.", Severity.Error));
	}

	/// <inheritdoc/>
	public override IEnumerable<Token> Lex()
	{
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
	}

	private Result<Token> ScanNumber(int startLoc)
	{
		bool dot = false;

		while (cursor < buffer.Length && (Char.IsAsciiDigit(buffer[cursor]) || buffer[cursor] == '_' || buffer[cursor] == '.'))
		{
			if (buffer[cursor] == '.')
			{
				if (dot)
					return Result.Success(new Token(TokenKind.Number, null, new(startLoc, cursor)));

				else
					dot = true;
			}

			cursor++;
		}

		return Result.Success(new Token(TokenKind.Number, null, new(startLoc, cursor)));
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

	private Result<Token> ScanIdentifierOrKeyword(int startLoc)
	{
		while (cursor < buffer.Length && (Char.IsAsciiLetterOrDigit(buffer[cursor]) || buffer[cursor] == '_'))
			cursor++;

		Range range = startLoc..cursor;

		if (Keywords.Contains(buffer[range]))
		{
			var token = new Token(GetKeywordTokenKind(buffer[range]), null, range); // is keyword

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
