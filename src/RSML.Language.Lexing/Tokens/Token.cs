using System;

namespace OceanApocalypse.RSML.Language.Lexing.Tokens;


/// <summary>
/// Represents a RSML token.
/// </summary>
/// <param name="Kind">An integer that identifies the type of token.</param>
/// <param name="Value">The token's value.</param>
/// <param name="Range">The range where the token occurs.</param>
public record struct Token(TokenKind Kind, object? Value, Range Range)
{
	/// <summary>
	/// Empty token. Used when something goes wrong.
	/// </summary>
	public readonly static Token Empty = new(TokenKind.Unknown, null, new());
}
