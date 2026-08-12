using OceanApocalypse.RSML.Abstractions.Diagnostics;
using OceanApocalypse.RSML.Language.Lexing;
using OceanApocalypse.RSML.Language.Lexing.Utf8;

namespace OceanApocalypse.RSML.Language.Tests.Lexing.Performance;

public class Utf8LexerUnitTest : LexerUnitTest
{
    protected override ILexer CreateLexer()
    {
        var collector = new DiagnosticCollector();
        return new Utf8Lexer(collector);
    }

    [Fact]
    public void Placeholder() => Assert.True(true); // todo: remove placeholder
}
