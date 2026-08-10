using System;
using System.Buffers;
using System.Collections.Generic;

using OceanApocalypse.RSML.Abstractions.Diagnostics;
using OceanApocalypse.RSML.Abstractions.Toolchain;
using OceanApocalypse.RSML.Language.Lexing.Tokens;


namespace OceanApocalypse.RSML.Language.Lexing;

/// <summary>
/// Represents a lexer for RSML.
/// </summary>
public interface ILexer<TInput> : IToolchainComponent
	where TInput : unmanaged, IEquatable<TInput>
{
	/// <summary>
	/// Tokenizes a source passed to the lexer.
	/// </summary>
	/// <returns>The tokens.</returns>
	IEnumerable<Token> Lex(ReadOnlySequence<TInput> data);

	/// <summary>
	/// Returns the next token.
	/// </summary>
	/// <returns>The next token.</returns>
	Result<Token> GetNextToken(ref SequenceReader<TInput> reader);
}
