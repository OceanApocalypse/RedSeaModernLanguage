using System.Collections.Generic;

using OceanApocalypse.RSML.Language.Lexing.Tokens;
using OceanApocalypse.RSML.Abstractions.Diagnostics;
using OceanApocalypse.RSML.Abstractions.Sources;

namespace OceanApocalypse.RSML.Language.Lexing;

/// <summary>
/// An implementation of a RSML lexer backed by a scanner.
/// </summary>
/// <param name="scanner">A scanner.</param>
/// <param name="diagnostics">A collector for all emitted diagnostics.</param>
public class ScannerLexer(IScanner scanner, DiagnosticCollector diagnostics) : Lexer
{
	/// <inheritdoc/>
	public override Result<Token> GetNextToken() => throw new System.NotImplementedException();

	/// <inheritdoc/>
	public override IEnumerable<Token> Lex() => throw new System.NotImplementedException();
}
