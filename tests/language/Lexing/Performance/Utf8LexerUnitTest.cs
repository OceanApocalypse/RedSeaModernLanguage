using System.Data;
using System.Linq;

using OceanApocalypse.RSML.Abstractions.Diagnostics;
using OceanApocalypse.RSML.Language.Lexing.Tokens;
using OceanApocalypse.RSML.Language.Lexing.Utf8;

namespace OceanApocalypse.RSML.Language.Tests.Lexing.Performance;

public class Utf8LexerUnitTest
{
    private static Utf8Lexer CreateLexer()
    {
        var diagnostics = new DiagnosticCollector();
        return new(diagnostics);
    }

	[Theory]
	[InlineData("# Simple comment, does it skip?")]
	[InlineData("#NoWhitespaceOverHere")]
	[InlineData("  #  T h i s  f e e l s  o m i n o u s ! !\n\n\n# $host.systemName == \"linux\"\n\n\n\n\r\n\n\n\r")]
	[InlineData("\t# This is seriously a very  \r\n       # dumb comment")]
	public void GetNextToken_SkipsCommentsIfNotConfigured(string data)
	{
		using var lexer = CreateLexer();
		lexer.Inject(new() { EmitComments = false });

		var tokens = (lexer.Lex(data) as Token[])?.ToArray();
		Assert.Single(tokens);
		Assert.Equal(TokenKind.Eof, tokens[0].Kind);
	}

	[Theory]
    [InlineData("# Simple comment, does it skip?", 1)]
    [InlineData("#NoWhitespaceOverHere", 1)]
    [InlineData("  #  T h i s  f e e l s  o m i n o u s ! !\n\n\n# $host.systemName == \"linux\"\n\n\n\n\r\n\n\n\r", 2)]
    [InlineData("\t# This is seriously a very  \r\n       # dumb comment", 2)]
    public void GetNextToken_DoesNotSkipCommentsIfConfigured(string data, int commentAmount)
    {
        using var lexer = CreateLexer();
        lexer.Inject(new() { EmitComments = true });

        var tokens = (lexer.Lex(data) as Token[])?.ToArray();
        Assert.Equal(commentAmount + 1, tokens.Length);
		
		for (int i = 0; i < commentAmount; i++)
			Assert.Equal(TokenKind.Comment, tokens[i].Kind);

		Assert.Equal(TokenKind.Eof, tokens[^1].Kind);
    }

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

    [Fact]
    public void Inject_FailsIfUsed()
    {
        using var lexer = CreateLexer();
        lexer.Lex("# This is a very dumb comment, seriously!!");
        Assert.Throws<ReadOnlyException>(() => lexer.Inject(new() { MaximumAllowedFailuresPerComponent = 1234 }));
        Assert.Equal(100, lexer.Configuration.MaximumAllowedFailuresPerComponent);
    }
}
