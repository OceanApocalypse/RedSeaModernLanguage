using System;

using OceanApocalypse.RSML.Abstractions;

namespace OceanApocalypse.RSML.Toolchain.Extensibility.Execution;

/// <summary>
/// The base type that deals with evaluating, interpreting and executing RSML.
/// </summary>
public abstract class Interpreter : IToolchainComponent
{
	// todo: add necessary content to IInterpreter
	private bool isDisposed;

	/// <inheritdoc/>
	public bool IsMutable { get; protected set; } = true;

	/// <inheritdoc/>
	public ToolchainConfigurations Configuration { get; protected set; }

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc/>
	public void Freeze() => IsMutable = false;

	/// <inheritdoc/>
	public void Inject(ToolchainConfigurations configuration) => throw new NotImplementedException();

	/// <summary>
	/// Disposes of both managed and unmanaged resources.
	/// </summary>
	/// <param name="disposing">When set to <c>false</c>, disposes of unmanaged resources only.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
			return;

		// note: dispose of managed if disposing is true

		isDisposed = true;
	}
}
