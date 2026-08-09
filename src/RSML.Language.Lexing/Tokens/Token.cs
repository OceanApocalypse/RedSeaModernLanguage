using System;
using System.Runtime.CompilerServices;

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

	/// <summary>
	/// Gets the token kind that applies to the given keyword or keyword modifier.
	/// </summary>
	/// <param name="keyword">The keyword or modifier.</param>
	/// <returns>The matching token kind.</returns>
	public static TokenKind GetKeywordKind(scoped ReadOnlySpan<char> keyword) => keyword switch
	{
		// keywords
		"as" => TokenKind.AsKeyword,
		"end" => TokenKind.EndKeyword,
		"if" => TokenKind.IfKeyword,
		"let" => TokenKind.LetKeyword,
		"region" => TokenKind.RegionKeyword,
		"requires" => TokenKind.RequiresKeyword,
		"return" => TokenKind.ReturnKeyword,
		"struct" => TokenKind.StructKeyword,
		"type" => TokenKind.TypeKeyword,

		// modifiers
		"fn" => TokenKind.FunctionModifier,
		"mut" => TokenKind.MutableModifier,
		"previous" => TokenKind.PreviousModifier,

		_ => TokenKind.Unknown,
	};

	/// <summary>
	/// Gets the token kind that applies to the given punctuation symbol.
	/// </summary>
	/// <param name="punctuation">The punctuation character.</param>
	/// <param name="peekedChar">
	/// The character that follows <paramref name="punctuation"/>.
	/// Set to null if out of bounds. Default is null.
	/// </param>
	/// <returns></returns>
	public static TokenKind GetPunctuationKind(char punctuation, char? peekedChar = null) => punctuation switch
	{
		// math operations
		'+' => TokenKind.Plus,
		'-' => TokenKind.Minus,
		'*' => TokenKind.Star,
		'/' => TokenKind.Slash,

		// equality
		'=' when peekedChar is '=' => TokenKind.EqualToOperator,
		'!' when peekedChar is '=' => TokenKind.NotEqualToOperator,
		'>' when peekedChar is '=' => TokenKind.GreaterThanOrEqualToOperator,
		'<' when peekedChar is '=' => TokenKind.LessThanOrEqualToOperator,
		'>' => TokenKind.GreaterThanOperator,
		'<' => TokenKind.LessThanOperator,

		// boolean logic
		'&' when peekedChar is '&' => TokenKind.LogicAndOperator,
		'|' when peekedChar is '|' => TokenKind.LogicOrOperator,
		'!' => TokenKind.NotOperator,

		'=' => TokenKind.AssignmentOperator,

		// reserved
		'&' => TokenKind.Unknown,
		'|' => TokenKind.Unknown,

		_ => TokenKind.Unknown
	};

	/// <summary>
	/// Returns <c>true</c> if the given token kind matches a valid keyword that is not a modifier.
	/// </summary>
	/// <param name="kind">The token kind to check against.</param>
	/// <returns>True if the kind is a keyword.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyKeyword(TokenKind kind) =>
		IsKeywordOrModifier(kind) && !IsStrictlyKeywordModifier(kind);

	/// <summary>
	/// Returns <c>true</c> if the given token kind matches a valid keyword modifier.
	/// </summary>
	/// <param name="kind">The token kind to check against.</param>
	/// <returns>True if the kind is a keyword modifier.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictlyKeywordModifier(TokenKind kind) =>
		kind is TokenKind.PreviousModifier or TokenKind.MutableModifier or TokenKind.FunctionModifier;

	/// <summary>
	/// Returns <c>true</c> if the given token kind points to an identifier, be it
	/// from the standard library or not.
	/// </summary>
	/// <param name="kind">The token kind to check against.</param>
	/// <returns>True if the kind is an identifier.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsIdentifier(TokenKind kind) =>
		kind is TokenKind.Identifier or TokenKind.StandardLibraryIdentifier;

	/// <summary>
	/// Returns <c>true</c> if the given token kind points to any keyword, be it
	/// a keyword or a keyword modifier.
	/// </summary>
	/// <param name="kind">The token kind to check against.</param>
	/// <returns>True if the kind is a keyword or modifier.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeywordOrModifier(TokenKind kind) =>
		kind is >= TokenKind.ReturnKeyword and <= TokenKind.StructKeyword;
}
