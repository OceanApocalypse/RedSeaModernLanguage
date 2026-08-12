using System;

using OceanApocalypse.RSML.Language.Lexing.Tokens;

namespace OceanApocalypse.RSML.Language.Lexing.Utf8;

/// <summary>
/// Represents a lexer with attributes or methods that utilize the unsafe context.
/// </summary>
[CLSCompliant(false)]
public unsafe interface IUnsafeLexer : IUtf8Lexer
{
    /// <summary>
	/// Tokenizes an array of bytes passed to the lexer, with UTF-8 encoding.
	/// </summary>
	/// <returns>The tokens.</returns>
    [CLSCompliant(false)]
    Token* Lex(byte* data, int charCount);
}
