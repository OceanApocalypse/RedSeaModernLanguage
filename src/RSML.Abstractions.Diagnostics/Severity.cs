namespace OceanApocalypse.RSML.Abstractions.Diagnostics;

/// <summary>
/// The severity of an error.
/// </summary>
public enum Severity
{
	/// <summary>
	/// No severity information.
	/// </summary>
	None,

	/// <summary>
	/// Messages and hints.
	/// </summary>
	Message,

	/// <summary>
	/// Non-critical warnings.
	/// </summary>
	Warning,

	/// <summary>
	/// Non-critical errors, such as style errors.
	/// </summary>
	Error,

	/// <summary>
	/// Fatal error that should abort the executing
	/// toolchain component.
	/// </summary>
	Critical
}
