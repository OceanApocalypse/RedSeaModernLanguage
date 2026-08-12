using System.Buffers;
using System.Collections.Generic;

using OceanApocalypse.RSML.Abstractions.Diagnostics;
using OceanApocalypse.RSML.Language.Lexing.Tokens;


namespace OceanApocalypse.RSML.Language.Lexing.Utf8;

/// <summary>
/// Represents a UTF-8 lexer for RSML.
/// </summary>
public interface IUtf8Lexer : ILexer
{
	/// <summary>
	/// Tokenizes a source passed to the lexer.
	/// </summary>
	/// <returns>The tokens.</returns>
	IEnumerable<Token> Lex(ReadOnlySequence<byte> data);

	/// <summary>
	/// Returns the next token.
	/// </summary>
	/// <param name="reader">The reader whose data to read.</param>
	/// <param name="currentPosition">The current expected position.</param> 
	/// <returns>The next token.</returns>
	Result<Token> GetNextToken(ref SequenceReader<byte> reader, ref AbsolutePosition currentPosition);
}
