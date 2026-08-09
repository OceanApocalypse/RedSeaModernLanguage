using System;


namespace OceanApocalypse.RSML.Abstractions;

/// <summary>
/// Configuration options for a <see cref="IToolchainComponent"/>.
/// </summary>
[Flags]
public enum ToolchainConfigurations
{
	/// <summary>
	/// Optimizes the toolchain pipeline by disabling extension processing.
	/// </summary>
	/// <remarks>
	/// :::warning
	/// This completely disables extensions, but does not warn you if there are active extensions,
	/// meaning sometimes you might be wondering why your extension is not working when, in reality,
	/// you've enabled this configuration.
	/// :::
	/// 
	/// :::tip
	/// This configuration is automatically enabled when no extensions are enabled.
	/// :::
	/// </remarks>
	DisableExtensionProcessing = 1,

	/// <summary>
	/// Only allows OceanApocalypse extensions, leading to an error if any non-OAS extension is active.
	/// </summary>
	/// <remarks>
	/// :::note
	/// When used alongside <see cref="IgnoreAllExtensibilityErrors"/>, the non-OAS extensions will be disabled, but
	/// no errors will be thrown.
	/// :::
	/// </remarks>
	AllowOnlyOASExtensions = 2,

	/// <summary>
	/// Ignores all errors caused by broken or faulty extensions.
	/// </summary>
	IgnoreBrokenExtensions = 4,

	/// <summary>
	/// Ignores all errors caused by injecting already injected extensions.
	/// </summary>
	IgnoreDuplicatedExtensions = 8,

	/// <summary>
	/// Ignores all errors thrown during pipeline creation and pipeline execution.
	/// </summary>
	/// <remarks>
	/// :::danger
	/// This option is only needed in beyond extremely rare occasions.
	/// It emulates RSML v1.x.x behavior.
	/// :::
	/// </remarks>
	IgnoreAllExtensibilityErrors = IgnoreBrokenExtensions | IgnoreDuplicatedExtensions
}
