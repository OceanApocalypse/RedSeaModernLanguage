using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using OceanApocalypse.RSML.Abstractions;

namespace OceanApocalypse.RSML.Language.Lexing.Tokens;


/// <summary>
/// Represents a RSML token.
/// </summary>
/// <param name="Kind">An integer that identifies the type of token.</param>
/// <param name="StartOffset">The offset at which the token begins.</param>
/// <param name="Length">The token's length.</param>
[StructLayout(LayoutKind.Sequential)]
public record struct Token(TokenKind Kind, long StartOffset, long Length)
{
	/// <summary>
	/// Any keyword with anything that exceeds this length immediately skips the keyword check.
	/// </summary>
	private const int MaxKeywordLength = 32;

	/// <summary>
	/// Empty token. Used when something goes wrong.
	/// </summary>
	public readonly static Token Empty = new(TokenKind.Unknown, 0L, 0L);

	/// <summary>
	/// The available keywords and keyword modifiers.
	/// </summary>
	public readonly static FrozenSet<string> Keywords = [
		"return", "if", "requires", "end", "previous", "region", "let",
		"mut", "fn", "exec", "type", "as", "struct",
		"class", "interface" // these 2 are reserved
	];

	/// <summary>
	/// Gets the token kind that applies to the given keyword or keyword modifier.
	/// </summary>
	/// <param name="sequence">The keyword or modifier sequence.</param>
	/// <returns>The matching token kind.</returns>
	public static TokenKind GetKeywordKind(ReadOnlySequence<byte> sequence)
	{
		if (sequence.Length > MaxKeywordLength)
			return TokenKind.Unknown; // we love failing fast

		Span<char> keywordBuffer = stackalloc char[MaxKeywordLength];
		int keywordLength = sequence.SafelyDecodeToCharacterSpan(keywordBuffer);

		return keywordBuffer[..keywordLength] switch
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
	}

	/// <summary>
	/// Gets the token kind that applies to the given punctuation symbol.
	/// </summary>
	/// <param name="punctuation">The punctuation character.</param>
	/// <param name="peekedChar">
	/// The character that follows <paramref name="punctuation"/>.
	/// Set to 0 if out of bounds/not punctuation.
	/// </param>
	/// <returns></returns>
	public static TokenKind GetPunctuationKind(byte punctuation, byte peekedChar) => punctuation switch
	{
		// math operations
		(byte)'+' => TokenKind.Plus,
		(byte)'-' => TokenKind.Minus,
		(byte)'*' => TokenKind.Star,
		(byte)'/' => TokenKind.Slash,

		// equality
		(byte)'=' when peekedChar is (byte)'=' => TokenKind.EqualToOperator,
		(byte)'!' when peekedChar is (byte)'=' => TokenKind.NotEqualToOperator,
		(byte)'>' when peekedChar is (byte)'=' => TokenKind.GreaterThanOrEqualToOperator,
		(byte)'<' when peekedChar is (byte)'=' => TokenKind.LessThanOrEqualToOperator,
		(byte)'>' => TokenKind.GreaterThanOperator,
		(byte)'<' => TokenKind.LessThanOperator,

		// boolean logic
		(byte)'&' when peekedChar is (byte)'&' => TokenKind.LogicAndOperator,
		(byte)'|' when peekedChar is (byte)'|' => TokenKind.LogicOrOperator,
		(byte)'!' => TokenKind.NotOperator,

		(byte)'=' => TokenKind.AssignmentOperator,

		// reserved
		(byte)'&' => TokenKind.Unknown,
		(byte)'|' => TokenKind.Unknown,

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
