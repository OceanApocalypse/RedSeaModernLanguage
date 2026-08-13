using System;

using OceanApocalypse.RSML.Abstractions.Toolchain;
using OceanApocalypse.RSML.Language.Lexing;
using OceanApocalypse.RSML.Toolchain.Tests;

namespace OceanApocalypse.RSML.Language.Tests.Lexing.Performance;

public abstract class LexerUnitTest : ToolchainUnitTest
{
    protected static ILexer AsLexerIfLexer<TComp>(TComp component)
        where TComp : IToolchainComponent =>
        component is not ILexer lexer ? throw new Exception("The component is not a lexer.") : lexer;
}
