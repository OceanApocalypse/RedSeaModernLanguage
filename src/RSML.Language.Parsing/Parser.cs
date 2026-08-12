using System;

using OceanApocalypse.RSML.Abstractions.Toolchain;

namespace OceanApocalypse.RSML.Language.Parsing;

/// <summary>
/// The base type that deals with parsing tokens and turning them into an organized tree.
/// </summary>
public abstract class Parser : IParser
{
	// todo: make this implement IParser correctly

	private bool isDisposed;

	/// <inheritdoc/>
	ToolchainConfiguration IToolchainComponent.Configuration => throw new NotImplementedException();

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc/>
	public void Inject(ToolchainConfiguration configuration) => throw new NotImplementedException();

	/// <summary>
	/// Disposes of both managed and unmanaged resources.
	/// </summary>
	/// <param name="disposing">When set to <c>false</c>, disposes of unmanaged resources only.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
			return;

		// note: dispose of managed resources here if disposing is true

		isDisposed = true;
	}
}
