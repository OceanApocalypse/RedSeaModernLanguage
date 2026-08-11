using System.Collections.Generic;

using OceanApocalypse.RSML.Abstractions.Toolchain;
using OceanApocalypse.RSML.Language.Lexing.Tokens;

namespace OceanApocalypse.RSML.Language.Lexing;

/// <summary>
/// Represents a lexer tasked with tokenizing RSML code.
/// </summary>
public interface ILexer : IToolchainComponent
{
    /// <summary>
	/// Tokenizes a string passed to the lexer.
	/// </summary>
	/// <returns>The tokens.</returns>
	IEnumerable<Token> Lex(string? data);

    /// <summary>
	/// Tokenizes an array of characters passed to the lexer.
	/// </summary>
	/// <returns>The tokens.</returns>
	IEnumerable<Token> Lex(char[] data);
}
