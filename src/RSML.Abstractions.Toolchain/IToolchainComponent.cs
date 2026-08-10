using System;
using System.Collections.Generic;

namespace OceanApocalypse.RSML.Abstractions.Toolchain;

/// <summary>
/// A component of the RSML toolchain.
/// </summary>
public interface IToolchainComponent : IDisposable
{
	/// <summary>
	/// Configurations for the toolchain component.
	/// </summary>
	ToolchainConfiguration Configuration { get; }

	/// <summary>
	/// Injects a configuration into the toolchain component, modifying it.
	/// </summary>
	/// <param name="configuration">The configuration to inject.</param>
	void Inject(ToolchainConfiguration configuration);
}
