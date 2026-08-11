using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using OceanApocalypse.RSML.Abstractions.Diagnostics;
using OceanApocalypse.RSML.Abstractions.Toolchain;
using OceanApocalypse.RSML.Language.Lexing.Tokens;


namespace OceanApocalypse.RSML.Language.Lexing.Utf8;

/// <summary>
/// Represents a UTF-8 lexer for RSML.
/// </summary>
public interface IUtf8Lexer<TInput> : ILexer
	where TInput : unmanaged, IEquatable<TInput>
{
	/// <summary>
	/// Tokenizes a source passed to the lexer.
	/// </summary>
	/// <returns>The tokens.</returns>
	IEnumerable<Token> Lex(ReadOnlySequence<TInput> data);

	/// <summary>
	/// Tokenizes a source passed to the lexer asynchronously.
	/// </summary>
	/// <param name="stream">The source as a byte stream</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The tokens.</returns>
	Task<IEnumerable<Token>> LexAsync(PipeStream stream, CancellationToken? cancellationToken = default);

	/// <summary>
	/// Returns the next token.
	/// </summary>
	/// <param name="reader">The reader whose data to read.</param>
	/// <param name="currentPosition">The current expected position.</param> 
	/// <returns>The next token.</returns>
	Result<Token> GetNextToken(ref SequenceReader<TInput> reader, ref AbsolutePosition currentPosition);
}
