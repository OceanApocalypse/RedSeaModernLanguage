using System.Collections.Generic;

using OceanApocalypse.RSML.Language.Lexing.Tokens;
using OceanApocalypse.RSML.Abstractions;
using OceanApocalypse.RSML.Abstractions.Diagnostics;


namespace OceanApocalypse.RSML.Language.Lexing;

/// <summary>
/// Represents a lexer for RSML.
/// </summary>
/// <remarks>
/// :::tip[Avoid starting from scratch]
/// If you want to add content on top of a lexer, without overriding
/// the extra functionality it adds, you might want to take a look at
/// <see cref="Lexer"/>.
/// :::
/// </remarks>
public interface ILexer : IToolchainComponent
{
	/// <summary>
	/// Tokenizes a source passed to the lexer.
	/// </summary>
	/// <returns>The tokens.</returns>
	IEnumerable<Token> Lex();

	/// <summary>
	/// Returns the next token.
	/// </summary>
	/// <returns>The next token.</returns>
	Result<Token> GetNextToken();
}
