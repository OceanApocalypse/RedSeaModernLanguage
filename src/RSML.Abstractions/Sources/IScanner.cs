using System;


namespace OceanApocalypse.RSML.Abstractions.Sources;

/// <summary>
/// Represents a sequential scanner.
/// </summary>
public interface IScanner : IDisposable, IEquatable<IScanner?>
{
	// todo: planned for v3.0.0-prerelease2

	/// <summary>
	/// The current index of the cursor.
	/// </summary>
	int CursorIndex { get; }

	/// <summary>
	/// Whether the source is completely empty.
	/// </summary>
	bool IsEmpty { get; }

	/// <summary>
	/// Whether the source can be mutated.
	/// </summary>
	bool IsReadOnly { get; }
}
