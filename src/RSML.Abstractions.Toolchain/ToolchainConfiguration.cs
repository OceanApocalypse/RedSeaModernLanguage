using System.Collections.Frozen;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace OceanApocalypse.RSML.Abstractions.Toolchain;

/// <summary>
/// Contains configurations for the entire toolchain.
/// </summary>
public record ToolchainConfiguration()
{
    private bool isFrozen;
    private readonly Dictionary<string, string> custom = [];

    /// <summary>
    /// The maximum amount of diagnostics needed for the toolchain to stop, per component.<br/>
    /// <c>0</c> does not limit failures.<br/>
    /// <c>1</c> simulates a fast fail: it's only recommended for CI purposes.
    /// </summary>
    public int MaximumAllowedFailuresPerComponent
    {
        get;
        set
        {
            ThrowIfFrozen();
            field = value;
        }
    } = 100;

    /// <summary>
    /// Whether or not the lexer should emit comment tokens.
    /// Setting to false is only recommended if only interpreting (no analysis tools).
    /// </summary>
    public bool EmitComments
    {
        get;
        set
        {
            ThrowIfFrozen();
            field = value;
        }
    } = true;

    /// <summary>
    /// The custom configurations.
    /// </summary>
    public IReadOnlyDictionary<string, string> CustomConfigurations => custom.ToFrozenDictionary();

    /// <summary>
    /// The default toolchain configuration.
    /// </summary>
    public static ToolchainConfiguration Default { get; } = new();

    /// <summary>
    /// Sets a custom configuration.
    /// </summary>
    /// <param name="key">The configuration's key.</param>
    /// <param name="value">The configuration's value.</param>
    public void SetCustom(string key, string value)
    {
        ThrowIfFrozen();
        custom[key] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfFrozen()
    {
        if (isFrozen)
            throw new ReadOnlyException("Configurations are frozen and cannot be changed.");
    }

    /// <summary>
    /// Freezes the configurations, preventing any future mutations.
    /// </summary>
    public void Freeze() => isFrozen = true;
}