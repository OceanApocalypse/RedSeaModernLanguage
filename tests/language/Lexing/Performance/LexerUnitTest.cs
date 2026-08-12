using System.Data;

using OceanApocalypse.RSML.Language.Lexing;

namespace OceanApocalypse.RSML.Language.Tests.Lexing.Performance;

public abstract class LexerUnitTest
{
    protected abstract ILexer CreateLexer();

    [Fact]
    public void Inject_ModifiesIfNotFrozen()
    {
        using var lexer = CreateLexer();
        lexer.Inject(new() { MaximumAllowedFailuresPerComponent = 1234 });
        Assert.Equal(1234, lexer.Configuration.MaximumAllowedFailuresPerComponent);
    }

    [Fact]
    public void Inject_FailsIfFrozen()
    {
        using var lexer = CreateLexer();
        lexer.Freeze();
        Assert.Throws<ReadOnlyException>(() => lexer.Inject(new() { MaximumAllowedFailuresPerComponent = 1234 }));
        Assert.Equal(100, lexer.Configuration.MaximumAllowedFailuresPerComponent);
    }
}
