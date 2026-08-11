namespace OceanApocalypse.RSML.Language.Lexing.Diagnostics;

internal static class LexerErrorCodes
{
	public const string GenericError = "RL0000";
	public const string FailedToLexToken = "RL0001";
	public const string UnterminatedStringLiteral = "RL0002";
	public const string FailedToIdentifyKeyword = "RL0003";
	public const string FailedToIdentifyPunctuation = "RL0004";
	public const string ExpectedStdIdentifier = "RL0005";
	public const string InvalidData = "RL0006";
}
