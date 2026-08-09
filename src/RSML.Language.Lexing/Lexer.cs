using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using OceanApocalypse.RSML.Language.Lexing.Tokens;
using OceanApocalypse.RSML.Abstractions;
using OceanApocalypse.RSML.Abstractions.Diagnostics;

namespace OceanApocalypse.RSML.Language.Lexing;

/// <summary>
/// The base class for implementations of RSML lexers and tokenizers.
/// </summary>
public abstract class Lexer : ILexer
{
	/// <summary>
	/// The RSML keywords defined in its language specification.
	/// Also contains reserved keywords.
	/// </summary>
	public static readonly ImmutableArray<string> Keywords = [
		// keywords
		"as", "end", "if", "let", "region", "requires", "return", "struct", "type",
		// modifiers
		"fn", "mut", "previous",
		// reserved keywords - not yet implemented but blocked from being used as identifiers
		"class", "interface"
	];

	private bool isDisposed;

	/// <inheritdoc/>
	public virtual ToolchainConfigurations Configuration { get; protected set; }

	/// <inheritdoc/>
	public virtual void Inject(ToolchainConfigurations configuration) => Configuration |= configuration;

	/// <inheritdoc/>
	public abstract IEnumerable<Token> Lex();

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes of both managed and unmanaged resources.
	/// </summary>
	/// <param name="disposing">When set to <c>false</c>, disposes of unmanaged resources only.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
			return;

		// dispose of managed stuff if disposing is true

		isDisposed = true;
	}

	/// <inheritdoc/>
	public abstract Result<Token> GetNextToken();
}
