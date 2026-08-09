using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OceanApocalypse.RSML.Abstractions.Diagnostics;

/// <summary>
/// A list of RSML toolchain errors.
/// </summary>
public sealed record DiagnosticCollector : IEnumerable<Diagnostic>
{
	/// <summary>
	/// Creates a new diagnostic collector.
	/// </summary>
	public DiagnosticCollector() => MinimumCriticalSeverity = Severity.Error;

	/// <summary>
	/// Creates a new diagnostic collector.
	/// </summary>
	/// <param name="minimumCriticalSeverity">The minimum diagnostic severity for a diagnostic to be considered critical.</param>
	public DiagnosticCollector(Severity minimumCriticalSeverity) => MinimumCriticalSeverity = minimumCriticalSeverity;

	private readonly List<Diagnostic> diagnostics = [];

	/// <summary>
	/// A property that indicates whether there are critical diagnostics.
	/// The toolchain should break if this is <c>true</c>.
	/// </summary>
	public bool HasCriticalErrors { get; private set; }

	/// <summary>
	/// The minimum diagnostic severity for a diagnostic to be considered critical.
	/// </summary>
	public Severity MinimumCriticalSeverity { get; }

	/// <summary>
	/// Adds an error to the list of errors.
	/// </summary>
	/// <param name="diagnostic"></param>
	public DiagnosticCollector Add(Diagnostic diagnostic)
	{
		diagnostics.Add(diagnostic);

		if ((byte)diagnostic.Severity >= (byte)MinimumCriticalSeverity)
			HasCriticalErrors = true;

		return this;
	}

	/// <summary>
	/// Clears the <see cref="DiagnosticCollector"/>, leaving it fully empty.
	/// </summary>
	public DiagnosticCollector Clear()
	{
		diagnostics.Clear();
		return this;
	}

	/// <summary>
	/// Returns all
	/// </summary>
	/// <returns></returns>
	public ImmutableArray<Diagnostic> GetAll() => diagnostics.ToImmutableArray();

	/// <inheritdoc/>
	public IEnumerator<Diagnostic> GetEnumerator() => diagnostics.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
