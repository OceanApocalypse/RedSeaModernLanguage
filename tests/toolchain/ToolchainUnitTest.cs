using System.Data;

using OceanApocalypse.RSML.Abstractions.Toolchain;

namespace OceanApocalypse.RSML.Toolchain.Tests;

public abstract class ToolchainUnitTest
{
    protected abstract IToolchainComponent CreateComponent();

    [Fact]
    public void Inject_ModifiesIfNotFrozen()
    {
        using var lexer = CreateComponent();
        lexer.Inject(new() { MaximumAllowedFailuresPerComponent = 1234 });
        Assert.Equal(1234, lexer.Configuration.MaximumAllowedFailuresPerComponent);
    }

    [Fact]
    public void Inject_FailsIfFrozen()
    {
        using var lexer = CreateComponent();
        lexer.Freeze();
        Assert.Throws<ReadOnlyException>(() => lexer.Inject(new() { MaximumAllowedFailuresPerComponent = 1234 }));
        Assert.Equal(100, lexer.Configuration.MaximumAllowedFailuresPerComponent);
    }
}