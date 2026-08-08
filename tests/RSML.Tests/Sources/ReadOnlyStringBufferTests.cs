using System;
using System.Text;

using OceanApocalypse.RSML.Toolchain.Abstractions.Panic;
using OceanApocalypse.RSML.Toolchain.Abstractions.Sources;
using OceanApocalypse.RSML.Toolchain.Sources;

namespace OceanApocalypse.RSML.Tests.Sources;

/// <summary>
/// Tests for the official-provided <see cref="ReadOnlyStringBuffer"/> class.
/// </summary>
public class ReadOnlyStringBufferTests
{
	private const string TestString01 = "Hey\r\nThis\rIs\u2029A Test \n Method\r\n\r\n.\u2028";
	private const string TestString02 = "Hey\r\nThis\rIs\u2029A Test \n Method\r\n\r\n.";
	private const string TestString03 = "This  string     has a lotofwhitespace     charact\u2029\u2028ers out of\r\nnowhere !! ";
	private const string TestString04 = "this STRING MIXES a LOT\n of \u2029dIffereNT cas1ngs RAND0mLy!?";
	private const string TestString05 = "!rrrrrrrrr";
	private const string TestString06 = "r!!!!!!!!!";
	private const string TestString07 = "This tests the\nLine\rCount property\r\n";

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, 3, true)]  // H in "Hey"
	[InlineData(TestString01, 3, 0, true)]  // CR in "Hey\r\n"
	[InlineData(TestString01, 4, 0, false)]  // LF in "Hey\r\n"
	[InlineData(TestString01, 5, 4, false)]  // T in "This"
	[InlineData(TestString01, 6, 3, false)]  // h in "This"
	[InlineData(TestString01, 13, 7, false)] // A in "A Test"
	[InlineData(TestString01, 15, 5, false)] // T in "Test"
	[InlineData(TestString01, 22, 6, true)] // M in "Method"
	[InlineData(TestString01, 28, 0, true)] // First CR in "\r\n\r\n."
	[InlineData(TestString01, 29, 0, false)] // First LF in "\r\n\r\n."
	[InlineData(TestString01, 30, 0, true)] // Second CR in "\r\n\r\n."
	[InlineData(TestString01, 31, 0, false)] // Second LF in "\r\n\r\n."
	[InlineData(TestString01, 32, 1, false)] // Dot/point in "\r\n\r\n."
	[InlineData(TestString01, 33, 0, false)] // U2028 in ".\u2028"
	[InlineData(TestString01, 34, 0, false)] // End of file
	#endregion
	#region String ends without newline
	[InlineData(TestString02, 0, 3, true)]  // H in "Hey"
	[InlineData(TestString02, 3, 0, true)]  // CR in "Hey\r\n"
	[InlineData(TestString02, 4, 0, false)]  // LF in "Hey\r\n"
	[InlineData(TestString02, 5, 4, false)]  // T in "This"
	[InlineData(TestString02, 6, 3, false)]  // h in "This"
	[InlineData(TestString02, 13, 7, false)] // A in "A Test"
	[InlineData(TestString02, 15, 5, false)] // T in "Test"
	[InlineData(TestString02, 22, 6, true)] // M in "Method"
	[InlineData(TestString02, 28, 0, true)] // First CR in "\r\n\r\n."
	[InlineData(TestString02, 29, 0, false)] // First LF in "\r\n\r\n."
	[InlineData(TestString02, 30, 0, true)] // Second CR in "\r\n\r\n."
	[InlineData(TestString02, -2, 0, false)] // Second LF in "\r\n\r\n."
	[InlineData(TestString02, -1, 1, false)] // Dot/point in "\r\n\r\n."
	[InlineData(TestString02, 33, 0, false)] // End of file
	#endregion
	#region String with a lot of whitespace
	[InlineData(TestString03, 0, 50, false)]
	[InlineData(TestString03, 7, 43, false)]
	[InlineData(TestString03, 19, 31, false)]
	[InlineData(TestString03, 32, 18, false)]
	[InlineData(TestString03, 49, 1, false)]
	[InlineData(TestString03, 50, 0, false)]
	[InlineData(TestString03, 51, 0, false)]
	[InlineData(TestString03, 52, 10, true)]
	[InlineData(TestString03, 59, 3, true)]
	[InlineData(TestString03, 61, 1, true)]
	[InlineData(TestString03, 62, 0, true)]
	#endregion
	#region String with a single line
	[InlineData(TestString05, 0, 10, false)]
	[InlineData(TestString05, 9, 1, false)]
	[InlineData(TestString05, 10, 0, false)]
	[InlineData(TestString06, 0, 10, false)]
	[InlineData(TestString06, 3, 7, false)]
	[InlineData(TestString06, 6, 4, false)]
	[InlineData(TestString06, 8, 2, false)]
	[InlineData(TestString06, 9, 1, false)]
	[InlineData(TestString06, 10, 0, false)]
	#endregion
	public void CountUntilEndOfLine(string data, int index, int expectedCount, bool expectedCrLf)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(expectedCount, buffer.CountUntilEndOfLine(index < 0 ? new(-index, true) : (Index)index, out bool actualCrLf));
		Assert.Equal(expectedCrLf, actualCrLf);
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(2)]
	[InlineData(14)]
	[InlineData(99)]
	[InlineData(-10)]
	[InlineData(-1)]
	#endregion
	public void CountUntilEndOfLine_FailsIfEmpty(int index)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		bool isCrLf = true;
		Assert.Throws<BufferException>(() => buffer.CountUntilEndOfLine(index < 0 ? new(-index, true) : (Index)index, out isCrLf));
		Assert.False(isCrLf);
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 99)]
	[InlineData(TestString01, 35)]
	[InlineData(TestString01, -35)]
	[InlineData(TestString02, -36)]
	[InlineData(TestString05, -11)]
	#endregion
	public void CountUntilEndOfLine_FailsIfOutOfRange(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		bool isCrLf = true;
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CountUntilEndOfLine(index < 0 ? new(-index, true) : (Index)index, out isCrLf));
		Assert.False(isCrLf);
	}

	[Theory]
	#region Attributes
	[InlineData(TestString03, 0, 0)]  // T in "This"
	[InlineData(TestString03, 2, 0)]  // i in "This"
	[InlineData(TestString03, 3, 0)]  // s in "This"
	[InlineData(TestString03, 4, 2)]  // whitespace after "This"
	[InlineData(TestString03, 5, 1)]  // whitespace after "This"
	[InlineData(TestString03, 6, 0)]  // s in "string"
	[InlineData(TestString03, 9, 0)]  // i in "string"
	[InlineData(TestString03, 11, 0)] // g in "string"
	[InlineData(TestString03, 12, 5)] // whitespace after "string"
	[InlineData(TestString03, 14, 3)] // whitespace after "string"
	[InlineData(TestString03, 15, 2)] // whitespace after "string"
	[InlineData(TestString03, 16, 1)] // whitespace after "string"
	[InlineData(TestString03, 20, 1)] // whitespace after "has"
	[InlineData(TestString03, 21, 0)] // "a" after "has"
	[InlineData(TestString03, 28, 0)] // w in "lotofwhitespace"
	[InlineData(TestString03, 38, 5)] // whitespace after "lotofwhitespace"
	[InlineData(TestString03, 39, 4)] // whitespace after "lotofwhitespace"
	[InlineData(TestString03, 42, 1)] // whitespace after "lotofwhitespace"
	[InlineData(TestString03, 46, 0)] // r in "charact\u2029\u2028"
	[InlineData(TestString03, 50, 2)] // U2029 in "charact\u2029\u2028"
	[InlineData(TestString03, 51, 1)] // U2028 in "charact\u2029\u2028"
	[InlineData(TestString03, 52, 0)] // e in "\u2029\u2028ers"
	[InlineData(TestString03, 59, 1)] // whitespace after "out"
	[InlineData(TestString03, 62, 2)] // CR in "\r\nnowhere"
	[InlineData(TestString03, 63, 1)] // LF in "\r\nnowhere"
	[InlineData(TestString03, 65, 0)] // o in "nowhere"
	[InlineData(TestString03, 69, 0)] // r in "nowhere"
	[InlineData(TestString03, 70, 0)] // last e in "nowhere"
	[InlineData(TestString03, 71, 1)] // whitespace after "nowhere"
	[InlineData(TestString03, 73, 0)] // second exclamation mark
	[InlineData(TestString03, 74, 1)] // whitespace after exclamation marks
	[InlineData(TestString03, 75, 0)] // End of file
	#endregion
	public void CountUntilNotWhitespace(string data, int index, int expectedCount)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(expectedCount, buffer.CountUntilNotWhitespace(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(2)]
	[InlineData(14)]
	[InlineData(99)]
	[InlineData(-10)]
	#endregion
	public void CountUntilNotWhitespace_FailsIfEmpty(int index)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.Throws<BufferException>(() => buffer.CountUntilNotWhitespace(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 99)]
	[InlineData(TestString01, 35)]
	[InlineData(TestString01, -35)]
	#endregion
	public void CountUntilNotWhitespace_FailsIfOutOfRange(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CountUntilNotWhitespace(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString03, 0, 4)]  // T in "This"
	[InlineData(TestString03, 2, 2)]  // i in "This"
	[InlineData(TestString03, 3, 1)]  // s in "This"
	[InlineData(TestString03, 4, 0)]  // whitespace after "This"
	[InlineData(TestString03, 5, 0)]  // whitespace after "This"
	[InlineData(TestString03, 6, 6)]  // s in "string"
	[InlineData(TestString03, 9, 3)]  // i in "string"
	[InlineData(TestString03, 11, 1)] // g in "string"
	[InlineData(TestString03, 12, 0)] // whitespace after "string"
	[InlineData(TestString03, 14, 0)] // whitespace after "string"
	[InlineData(TestString03, 15, 0)] // whitespace after "string"
	[InlineData(TestString03, 16, 0)] // whitespace after "string"
	[InlineData(TestString03, 20, 0)] // whitespace after "has"
	[InlineData(TestString03, 21, 1)] // "a" after "has"
	[InlineData(TestString03, 28, 10)] // w in "lotofwhitespace"
	[InlineData(TestString03, 38, 0)] // whitespace after "lotofwhitespace"
	[InlineData(TestString03, 39, 0)] // whitespace after "lotofwhitespace"
	[InlineData(TestString03, 42, 0)] // whitespace after "lotofwhitespace"
	[InlineData(TestString03, 46, 4)] // r in "charact\u2029\u2028"
	[InlineData(TestString03, 50, 0)] // U2029 in "charact\u2029\u2028"
	[InlineData(TestString03, 51, 0)] // U2028 in "charact\u2029\u2028"
	[InlineData(TestString03, 52, 3)] // e in "\u2029\u2028ers"
	[InlineData(TestString03, 59, 0)] // whitespace after "out"
	[InlineData(TestString03, 62, 0)] // CR in "\r\nnowhere"
	[InlineData(TestString03, 63, 0)] // LF in "\r\nnowhere"
	[InlineData(TestString03, 65, 6)] // o in "nowhere"
	[InlineData(TestString03, 69, 2)] // r in "nowhere"
	[InlineData(TestString03, 70, 1)] // last e in "nowhere"
	[InlineData(TestString03, 71, 0)] // whitespace after "nowhere"
	[InlineData(TestString03, 73, 1)] // second exclamation mark
	[InlineData(TestString03, 74, 0)] // whitespace after exclamation marks
	[InlineData(TestString03, 75, 0)] // End of file
	#endregion
	public void CountUntilWhitespace(string data, int index, int expectedCount)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(expectedCount, buffer.CountUntilWhitespace(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(2)]
	[InlineData(14)]
	[InlineData(99)]
	[InlineData(-10)]
	#endregion
	public void CountUntilWhitespace_FailsIfEmpty(int index)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.Throws<BufferException>(() => buffer.CountUntilWhitespace(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 99)]
	[InlineData(TestString01, 35)]
	[InlineData(TestString01, -35)]
	#endregion
	public void CountUntilWhitespace_FailsIfOutOfRange(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CountUntilWhitespace(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01)]
	[InlineData(TestString02)]
	[InlineData(TestString03)]
	[InlineData(TestString04)]
	#endregion
	public void CountWhile_SameAsLengthIfAlwaysTrue(string data)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(data.Length, buffer.CountWhile(0, (_, _) => true));
	}

	[Theory]
	#region Regular string
	[InlineData(TestString03, 0, 8)] // T in "This
	[InlineData(TestString03, 2, 6)] // i in "This"
	[InlineData(TestString03, 5, 3)] // whitespace before "string"
	[InlineData(TestString03, 7, 1)] // t in "This  string"
	[InlineData(TestString03, 8, 0)] // r in "This  string"
	[InlineData(TestString03, 9, 37)] // i in "string"
	[InlineData(TestString03, 10, 36)]
	[InlineData(TestString03, 11, 35)]
	[InlineData(TestString03, 15, 31)]
	[InlineData(TestString03, 18, 28)]
	[InlineData(TestString03, 20, 26)]
	[InlineData(TestString03, 22, 24)]
	[InlineData(TestString03, 24, 22)]
	[InlineData(TestString03, 29, 17)]
	[InlineData(TestString03, 31, 15)]
	[InlineData(TestString03, 34, 12)]
	[InlineData(TestString03, 37, 9)]
	[InlineData(TestString03, 41, 5)]
	[InlineData(TestString03, 44, 2)]
	[InlineData(TestString03, 45, 1)]
	[InlineData(TestString03, 46, 0)]
	[InlineData(TestString03, 47, 6)] // second in "charact"
	[InlineData(TestString03, 48, 5)]
	[InlineData(TestString03, 50, 3)]
	[InlineData(TestString03, 51, 2)]
	[InlineData(TestString03, 53, 0)]
	[InlineData(TestString03, 54, 15)] // s in "ers "
	[InlineData(TestString03, 55, 14)]
	[InlineData(TestString03, 58, 11)]
	[InlineData(TestString03, 61, 8)]
	[InlineData(TestString03, 63, 6)]
	[InlineData(TestString03, 64, 5)]
	[InlineData(TestString03, 67, 2)]
	[InlineData(TestString03, 68, 1)]
	[InlineData(TestString03, 69, 0)]
	[InlineData(TestString03, 70, 5)]
	[InlineData(TestString03, 72, 3)] // after last r in the whole string
	[InlineData(TestString03, 74, 1)] // before End of file
	[InlineData(TestString03, 75, 0)] // End of file
	#endregion
	#region String with several r's
	[InlineData(TestString05, 0, 1)] // "!"
	[InlineData(TestString05, 1, 0)]
	[InlineData(TestString05, 4, 0)]
	[InlineData(TestString05, 7, 0)]
	[InlineData(TestString05, 9, 0)]
	[InlineData(TestString05, 10, 0)] // EOF
	#endregion
	#region String with a single r
	[InlineData(TestString06, 0, 0)] // "r"
	[InlineData(TestString06, 1, 9)]
	[InlineData(TestString06, 4, 6)]
	[InlineData(TestString06, 7, 3)]
	[InlineData(TestString06, 9, 1)]
	[InlineData(TestString06, 10, 0)] // EOF
	#endregion
	public void CountWhile_CountsWhileNotLowercaseR(string data, int index, int expectedCount)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(expectedCount, buffer.CountWhile(index < 0 ? new(-index, true) : (Index)index, (_, c) => c != 'r'));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString04, 0, 0)] // t in "this STRING"
	[InlineData(TestString04, 3, 0)] // s in "this STRING"
	[InlineData(TestString04, 4, 14)] // whitespace after "this"
	[InlineData(TestString04, 5, 13)] // S in "STRING"
	[InlineData(TestString04, 6, 12)] // T in "STRING"
	[InlineData(TestString04, 8, 10)] // I in "STRING"
	[InlineData(TestString04, 9, 9)] // N in "STRING"
	[InlineData(TestString04, 10, 8)] // G in "STRING"
	[InlineData(TestString04, 11, 7)] // whitespace after "STRING"
	[InlineData(TestString04, 12, 6)] // M in "MIXES"
	[InlineData(TestString04, 14, 4)] // X in "MIXES"
	[InlineData(TestString04, 16, 2)] // S in "MIXES"
	[InlineData(TestString04, 17, 1)] // whitespace after "MIXES"
	[InlineData(TestString04, 18, 0)] // "a" surrounded by whitespace
	[InlineData(TestString04, 19, 6)] // whitespace after sole "a"
	[InlineData(TestString04, 21, 4)] // O in "LOT"
	[InlineData(TestString04, 22, 3)] // T in "LOT"
	[InlineData(TestString04, 23, 2)] // LF after "LOT"
	[InlineData(TestString04, 24, 1)] // whitespace after "LOT\n"
	[InlineData(TestString04, 26, 0)] // f in "of"
	[InlineData(TestString04, 27, 2)] // whitespace after "of"
	[InlineData(TestString04, 28, 1)] // U2029 after "of "
	[InlineData(TestString04, 29, 0)] // d in "dIffereNT"
	[InlineData(TestString04, 30, 1)] // I in "dIffereNT"
	[InlineData(TestString04, 31, 0)] // first f in "dIffereNT"
	[InlineData(TestString04, 32, 0)] // second f in "dIffereNT"
	[InlineData(TestString04, 35, 0)] // second e in "dIffereNT"
	[InlineData(TestString04, 36, 3)] // N in "dIffereNT"
	[InlineData(TestString04, 37, 2)] // T in "dIffereNT"
	[InlineData(TestString04, 38, 1)] // whitespace after "dIffereNT"
	[InlineData(TestString04, 39, 0)] // c in "cas1ngs"
	[InlineData(TestString04, 42, 0)] // 1 in "cas1ngs"
	[InlineData(TestString04, 46, 5)] // whitespace after "cas1ngs"
	[InlineData(TestString04, 49, 2)] // N in "RAND0mLy"
	[InlineData(TestString04, 51, 0)] // 0 in "RAND0mLy"
	[InlineData(TestString04, 55, 0)] // exclamation mark
	[InlineData(TestString04, 56, 0)] // question mark
	[InlineData(TestString04, 57, 0)] // End of file
	#endregion
	public void CountWhile_CountsWhileUppercaseOrWhitespace(string data, int index, int expectedCount)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(expectedCount, buffer.CountWhile(index < 0 ? new(-index, true) : (Index)index, (_, c) => Char.IsWhiteSpace(c) || c is '\r' or '\n' or '\u2028' or '\u2029' || Char.IsUpper(c)));
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(2)]
	[InlineData(14)]
	[InlineData(99)]
	[InlineData(-10)]
	#endregion
	public void CountWhile_FailsIfEmpty(int index)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.Throws<BufferException>(() => buffer.CountWhile(index < 0 ? new(-index, true) : (Index)index, (_, _) => true));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 99)]
	[InlineData(TestString01, 35)]
	[InlineData(TestString01, -35)]
	#endregion
	public void CountWhile_FailsIfOutOfRange(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CountWhile(index < 0 ? new(-index, true) : (Index)index, (_, _) => true));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01)]
	[InlineData(TestString02)]
	[InlineData(TestString04)]
	[InlineData(TestString06)]
	#endregion
	public void EqualsBuffer(string data) => Assert.Equal(new ReadOnlyStringBuffer(data), new ReadOnlyStringBuffer(data));

	[Theory]
	#region Attributes
	[InlineData(TestString01)]
	[InlineData(TestString02)]
	[InlineData(TestString04)]
	[InlineData(TestString06)]
	#endregion
	public void EqualsMemory(string data) => Assert.True(new ReadOnlyStringBuffer(data).Equals(data.AsMemory()));

	[Theory]
	#region Attributes
	[InlineData(TestString01)]
	[InlineData(TestString02)]
	[InlineData(TestString04)]
	[InlineData(TestString06)]
	#endregion
	public void EqualsString(string data) => Assert.True(new ReadOnlyStringBuffer(data).Equals(data));

	[Theory]
	#region Attributes
	[InlineData(TestString01)]
	[InlineData(TestString02)]
	[InlineData(TestString04)]
	[InlineData(TestString06)]
	#endregion
	public void EqualsSpan(string data) => Assert.True(new ReadOnlyStringBuffer(data).Equals(data.AsSpan()));

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, 3)] // "Hey"
	[InlineData(TestString01, 1, 4)] // "This"
	[InlineData(TestString01, 2, 2)] // "Is"
	[InlineData(TestString01, 3, 7)] // "A Test "
	[InlineData(TestString01, 4, 7)] // " Method"
	[InlineData(TestString01, 5, 0)] // ""
	[InlineData(TestString01, 6, 1)] // "."
	#endregion
	#region String ends without newline
	[InlineData(TestString02, 0, 3)] // "Hey"
	[InlineData(TestString02, 1, 4)] // "This"
	[InlineData(TestString02, 2, 2)] // "Is"
	[InlineData(TestString02, 3, 7)] // "A Test "
	[InlineData(TestString02, 4, 7)] // " Method"
	[InlineData(TestString02, 5, 0)] // ""
	[InlineData(TestString02, 6, 1)] // "."
	#endregion
	public void GetLengthOfLine(string data, int lineNumber, int expectedLength)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		buffer.BuildCache();
		Assert.Equal(expectedLength, buffer.GetLengthOfLine(lineNumber));
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(20)]
	[InlineData(69)]
	[InlineData(136)]
	[InlineData(-4)]
	#endregion
	public void GetLengthOfLine_FailsIfEmpty(int lineNumber)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.Throws<BufferException>(() => buffer.GetLengthOfLine(lineNumber));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 8)]
	[InlineData(TestString01, -1)]
	[InlineData(TestString01, -4)]
	[InlineData(TestString01, 20)]
	[InlineData(TestString01, -20)]
	#endregion
	public void GetLengthOfLine_FailsIfOutOfRange(string data, int lineNumber)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetLengthOfLine(lineNumber));
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, 3)]  // H in "Hey"
	[InlineData(TestString01, 3, 3)]  // CR in "Hey\r\n"
	[InlineData(TestString01, 4, 3)]  // LF in "Hey\r\n"
	[InlineData(TestString01, 5, 4)]  // T in "This"
	[InlineData(TestString01, 6, 4)]  // h in "This"
	[InlineData(TestString01, 13, 7)] // A in "A Test"
	[InlineData(TestString01, 15, 7)] // T in "Test"
	[InlineData(TestString01, 22, 7)] // M in "Method"
	[InlineData(TestString01, 28, 7)] // First CR in "\r\n\r\n."
	[InlineData(TestString01, 29, 7)] // First LF in "\r\n\r\n."
	[InlineData(TestString01, 30, 0)] // Second CR in "\r\n\r\n."
	[InlineData(TestString01, 31, 0)] // Second LF in "\r\n\r\n."
	[InlineData(TestString01, 32, 1)] // Dot/point in "\r\n\r\n."
	[InlineData(TestString01, 33, 1)] // U2028 in ".\u2028"
	[InlineData(TestString01, 34, 0)] // End of file
	#endregion
	#region String ends without newline
	[InlineData(TestString02, 0, 3)]  // H in "Hey"
	[InlineData(TestString02, 3, 3)]  // CR in "Hey\r\n"
	[InlineData(TestString02, 4, 3)]  // LF in "Hey\r\n"
	[InlineData(TestString02, 5, 4)]  // T in "This"
	[InlineData(TestString02, 6, 4)]  // h in "This"
	[InlineData(TestString02, 13, 7)] // A in "A Test"
	[InlineData(TestString02, 15, 7)] // T in "Test"
	[InlineData(TestString02, 22, 7)] // M in "Method"
	[InlineData(TestString02, 28, 7)] // First CR in "\r\n\r\n."
	[InlineData(TestString02, 29, 7)] // First LF in "\r\n\r\n."
	[InlineData(TestString02, 30, 0)] // Second CR in "\r\n\r\n."
	[InlineData(TestString02, -2, 0)] // Second LF in "\r\n\r\n."
	[InlineData(TestString02, -1, 1)] // Dot/point in "\r\n\r\n."
	[InlineData(TestString02, 33, 0)] // End of file
	#endregion
	public void GetLengthOfLineFromIndex(string data, int index, int expectedLength)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(expectedLength, buffer.GetLengthOfLineFromIndex(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(20)]
	[InlineData(69)]
	[InlineData(136)]
	[InlineData(-4)]
	#endregion
	public void GetLengthOfLineFromIndex_FailsIfEmpty(int index)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.Throws<BufferException>(() => buffer.GetLengthOfLineFromIndex(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 99)]
	[InlineData(TestString01, 35)]
	[InlineData(TestString01, -35)]
	#endregion
	public void GetLengthOfLineFromIndex_FailsIfOutOfRange(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetLengthOfLineFromIndex(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, "Hey")]
	[InlineData(TestString01, 1, "This")]
	[InlineData(TestString01, 2, "Is")]
	[InlineData(TestString01, 3, "A Test ")]
	[InlineData(TestString01, 4, " Method")]
	[InlineData(TestString01, 5, "")]
	[InlineData(TestString01, 6, ".")]
	[InlineData(TestString01, 7, "")] // eof
	#endregion
	#region String ends without newline
	[InlineData(TestString02, 0, "Hey")]
	[InlineData(TestString02, 1, "This")]
	[InlineData(TestString02, 2, "Is")]
	[InlineData(TestString02, 3, "A Test ")]
	[InlineData(TestString02, 4, " Method")]
	[InlineData(TestString02, 5, "")]
	[InlineData(TestString02, 6, ".")]
	[InlineData(TestString02, 7, "")] // eof
	#endregion
	#region Single-line strings
	[InlineData(TestString05, 0, "!rrrrrrrrr")]
	[InlineData(TestString05, 1, "")] // eof
	[InlineData(TestString06, 0, "r!!!!!!!!!")]
	[InlineData(TestString06, 1, "")] // eof
	#endregion
	public void GetLine(string data, int lineNumber, string expectedLine)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(expectedLine, buffer.GetLine(lineNumber));
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(20)]
	[InlineData(69)]
	[InlineData(136)]
	[InlineData(-4)]
	#endregion
	public void GetLine_FailsIfEmpty(int lineNumber)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.Throws<BufferException>(() => buffer.GetLine(lineNumber));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 8)]
	[InlineData(TestString01, -1)]
	[InlineData(TestString01, -4)]
	[InlineData(TestString01, 20)]
	[InlineData(TestString01, -20)]
	[InlineData(TestString02, 8)]
	[InlineData(TestString05, 2)]
	#endregion
	public void GetLine_FailsIfOutOfRange(string data, int lineNumber)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetLine(lineNumber));
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, "Hey")]  // H in "Hey"
	[InlineData(TestString01, 2, "Hey")]  // y in "Hey\r\n"
	[InlineData(TestString01, 3, "Hey")]  // CR in "Hey\r\n"
	[InlineData(TestString01, 4, "Hey")]  // LF in "Hey\r\n"
	[InlineData(TestString01, 5, "This")]  // T in "This"
	[InlineData(TestString01, 6, "This")]  // h in "This"
	[InlineData(TestString01, 11, "Is")] // s in "Is"
	[InlineData(TestString01, 12, "Is")] // U2029 before "A Test"
	[InlineData(TestString01, 13, "A Test ")] // A in "A Test"
	[InlineData(TestString01, 15, "A Test ")] // T in "Test"
	[InlineData(TestString01, 22, " Method")] // M in "Method"
	[InlineData(TestString01, 28, " Method")] // First CR in "\r\n\r\n."
	[InlineData(TestString01, 29, " Method")] // First LF in "\r\n\r\n."
	[InlineData(TestString01, 30, "")] // Second CR in "\r\n\r\n."
	[InlineData(TestString01, 31, "")] // Second LF in "\r\n\r\n."
	[InlineData(TestString01, 32, ".")] // Dot/point in "\r\n\r\n."
	[InlineData(TestString01, 33, ".")] // U2028 in ".\u2028"
	[InlineData(TestString01, 34, "")] // End of file
	#endregion
	#region String ends without newline
	[InlineData(TestString02, 0, "Hey")]  // H in "Hey"
	[InlineData(TestString02, 2, "Hey")]  // y in "Hey\r\n"
	[InlineData(TestString02, 3, "Hey")]  // CR in "Hey\r\n"
	[InlineData(TestString02, 4, "Hey")]  // LF in "Hey\r\n"
	[InlineData(TestString02, 5, "This")]  // T in "This"
	[InlineData(TestString02, 6, "This")]  // h in "This"
	[InlineData(TestString02, 11, "Is")] // s in "Is"
	[InlineData(TestString02, 12, "Is")] // U2029 before "A Test"
	[InlineData(TestString02, 13, "A Test ")] // A in "A Test"
	[InlineData(TestString02, 15, "A Test ")] // T in "Test"
	[InlineData(TestString02, 22, " Method")] // M in "Method"
	[InlineData(TestString02, 28, " Method")] // First CR in "\r\n\r\n."
	[InlineData(TestString02, 29, " Method")] // First LF in "\r\n\r\n."
	[InlineData(TestString02, 30, "")] // Second CR in "\r\n\r\n."
	[InlineData(TestString02, -2, "")] // Second LF in "\r\n\r\n."
	[InlineData(TestString02, -1, ".")] // Dot/point in "\r\n\r\n."
	[InlineData(TestString02, 33, "")] // End of file
	#endregion
	public void GetLineFromIndex(string data, int index, string expectedLine)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(expectedLine, buffer.GetLineFromIndex(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 99)]
	[InlineData(TestString01, 35)]
	[InlineData(TestString01, -35)]
	#endregion
	public void GetLineFromIndex_FailsIfOutOfRange(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetLineFromIndex(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(20)]
	[InlineData(69)]
	[InlineData(136)]
	[InlineData(-4)]
	#endregion
	public void GetLineFromIndex_FailsIfEmpty(int index)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.Throws<BufferException>(() => buffer.GetLineFromIndex(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, 0)]  // H in "Hey"
	[InlineData(TestString01, 3, 0)]  // CR in "Hey\r\n"
	[InlineData(TestString01, 4, 0)]  // LF in "Hey\r\n"
	[InlineData(TestString01, 5, 1)]  // T in "This"
	[InlineData(TestString01, 6, 1)]  // h in "This"
	[InlineData(TestString01, 13, 3)] // A in "A Test"
	[InlineData(TestString01, 15, 3)] // T in "Test"
	[InlineData(TestString01, 22, 4)] // M in "Method"
	[InlineData(TestString01, 28, 4)] // First CR in "\r\n\r\n."
	[InlineData(TestString01, 29, 4)] // First LF in "\r\n\r\n."
	[InlineData(TestString01, 30, 5)] // Second CR in "\r\n\r\n."
	[InlineData(TestString01, 31, 5)] // Second LF in "\r\n\r\n."
	[InlineData(TestString01, 32, 6)] // Dot/point in "\r\n\r\n."
	[InlineData(TestString01, 33, 6)] // U2028 in ".\u2028"
	[InlineData(TestString01, 34, 7)] // End of file
	#endregion
	#region String ends without newline
	[InlineData(TestString02, 0, 0)]  // H in "Hey"
	[InlineData(TestString02, 3, 0)]  // CR in "Hey\r\n"
	[InlineData(TestString02, 4, 0)]  // LF in "Hey\r\n"
	[InlineData(TestString02, 5, 1)]  // T in "This"
	[InlineData(TestString02, 6, 1)]  // h in "This"
	[InlineData(TestString02, 13, 3)] // A in "A Test"
	[InlineData(TestString02, 15, 3)] // T in "Test"
	[InlineData(TestString02, 22, 4)] // M in "Method"
	[InlineData(TestString02, 28, 4)] // First CR in "\r\n\r\n."
	[InlineData(TestString02, 29, 4)] // First LF in "\r\n\r\n."
	[InlineData(TestString02, 30, 5)] // Second CR in "\r\n\r\n."
	[InlineData(TestString02, -2, 5)] // Second LF in "\r\n\r\n."
	[InlineData(TestString02, -1, 6)] // Dot/point in "\r\n\r\n."
	[InlineData(TestString02, 33, 7)] // EOF
	#endregion
	public void GetLineNumberFromIndex(string data, int index, int expectedLineNumber)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		buffer.BuildCache();
		Assert.Equal(expectedLineNumber, buffer.GetLineNumberFromIndex(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 99)]
	[InlineData(TestString01, 35)]
	[InlineData(TestString01, -35)]
	#endregion
	public void GetLineNumberFromIndex_FailsIfOutOfRange(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetLineNumberFromIndex(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(20)]
	[InlineData(69)]
	[InlineData(136)]
	[InlineData(-4)]
	#endregion
	public void GetLineNumberFromIndex_FailsIfEmpty(int index)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.Throws<BufferException>(() => buffer.GetLineNumberFromIndex(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, 0, 0)]  // H in "Hey"
	[InlineData(TestString01, 3, 0, 3)]  // CR in "Hey\r\n"
	[InlineData(TestString01, 4, 0, 4)]  // LF in "Hey\r\n"
	[InlineData(TestString01, 5, 1, 0)]  // T in "This"
	[InlineData(TestString01, 6, 1, 1)]  // h in "This"
	[InlineData(TestString01, 13, 3, 0)] // A in "A Test"
	[InlineData(TestString01, 15, 3, 2)] // T in "Test"
	[InlineData(TestString01, 22, 4, 1)] // M in "Method"
	[InlineData(TestString01, 28, 4, 7)] // First CR in "\r\n\r\n."
	[InlineData(TestString01, -5, 4, 8)] // First LF in "\r\n\r\n."
	[InlineData(TestString01, 30, 5, 0)] // Second CR in "\r\n\r\n."
	[InlineData(TestString01, -3, 5, 1)] // Second LF in "\r\n\r\n."
	[InlineData(TestString01, 32, 6, 0)] // Dot/point in "\r\n\r\n."
	[InlineData(TestString01, 33, 6, 1)] // U2028 in ".\u2028"
	#endregion
	#region String ends without newline
	[InlineData(TestString02, 0, 0, 0)]  // H in "Hey"
	[InlineData(TestString02, 3, 0, 3)]  // CR in "Hey\r\n"
	[InlineData(TestString02, 4, 0, 4)]  // LF in "Hey\r\n"
	[InlineData(TestString02, 5, 1, 0)]  // T in "This"
	[InlineData(TestString02, 6, 1, 1)]  // h in "This"
	[InlineData(TestString02, 13, 3, 0)] // A in "A Test"
	[InlineData(TestString02, 15, 3, 2)] // T in "Test"
	[InlineData(TestString02, 22, 4, 1)] // M in "Method"
	[InlineData(TestString02, 28, 4, 7)] // First CR in "\r\n\r\n."
	[InlineData(TestString02, 29, 4, 8)] // First LF in "\r\n\r\n."
	[InlineData(TestString02, 30, 5, 0)] // Second CR in "\r\n\r\n."
	[InlineData(TestString02, -2, 5, 1)] // Second LF in "\r\n\r\n."
	[InlineData(TestString02, -1, 6, 0)] // Dot/point in "\r\n\r\n."
	#endregion
	public void GetLocationDetails(string data, int index, int expectedLine, int expectedColumn)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		var location = buffer.GetLocationDetails(index < 0 ? new(-index, true) : (Index)index);

		Assert.Equal(expectedLine, location.Line);
		Assert.Equal(expectedColumn, location.Column);
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 99)]
	[InlineData(TestString01, 34)] // EOF is not accepted for this method
	[InlineData(TestString01, 35)]
	[InlineData(TestString01, -35)]
	#endregion
	public void GetLocationDetails_FailsIfOutOfRange(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetLocationDetails(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region Attributes
	[InlineData(0)]
	[InlineData(20)]
	[InlineData(69)]
	[InlineData(136)]
	[InlineData(-4)]
	#endregion
	public void GetLocationDetails_FailsIfEmpty(int index)
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.Throws<BufferException>(() => buffer.GetLocationDetails(index < 0 ? new(-index, true) : (Index)index));
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0)]
	[InlineData(TestString01, 3)]
	[InlineData(TestString01, 7)]
	[InlineData(TestString01, 8)]
	[InlineData(TestString01, 10)]
	[InlineData(TestString01, 12)]
	[InlineData(TestString01, 15)]
	[InlineData(TestString01, 21)]
	[InlineData(TestString01, 24)]
	[InlineData(TestString01, 26)]
	[InlineData(TestString01, 27)]
	[InlineData(TestString01, 30)]
	[InlineData(TestString01, 32)]
	[InlineData(TestString01, 33)]
	#endregion
	#region String ends with newline
	[InlineData(TestString02, 0)]
	[InlineData(TestString02, 2)]
	[InlineData(TestString02, 5)]
	[InlineData(TestString02, 9)]
	[InlineData(TestString02, 10)]
	[InlineData(TestString02, 12)]
	[InlineData(TestString02, 15)]
	[InlineData(TestString02, 19)]
	[InlineData(TestString02, 23)]
	[InlineData(TestString02, 27)]
	[InlineData(TestString02, 29)]
	[InlineData(TestString02, 31)]
	#endregion
	#region String with a lot of whitespace
	[InlineData(TestString03, 0)]
	[InlineData(TestString03, 2)]
	[InlineData(TestString03, 15)]
	[InlineData(TestString03, 29)]
	[InlineData(TestString03, 30)]
	[InlineData(TestString03, 42)]
	[InlineData(TestString03, 55)]
	[InlineData(TestString03, 69)]
	[InlineData(TestString03, 73)]
	#endregion
	#region String with a single line
	[InlineData(TestString05, 0)]
	[InlineData(TestString05, 2)]
	[InlineData(TestString05, 5)]
	[InlineData(TestString05, 9)]
	[InlineData(TestString06, 1)]
	[InlineData(TestString06, 4)]
	[InlineData(TestString06, 6)]
	[InlineData(TestString06, 8)]
	#endregion
	public void IndexAccessor(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(data[index], buffer[index]);
		Assert.Equal(data[index], buffer[index < 0 ? new Index(-index, true) : (Index)index]);
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01, 7)]
	[InlineData(TestString02, 7)]
	[InlineData(TestString03, 4)]
	[InlineData(TestString04, 3)]
	[InlineData(TestString05, 1)]
	[InlineData(TestString06, 1)]
	[InlineData(TestString07, 3)]
	#endregion
	public void LineCount_MatchesActualLineCountWithNoEof(string data, int expectedLineCount)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.Equal(expectedLineCount, buffer.LineCount);
	}

	[Theory]
	#region Attributes
	[InlineData(TestString02)]
	[InlineData(TestString03)]
	[InlineData(TestString04)]
	[InlineData(TestString06)]
	#endregion
	public void Constructor_CharArray(string data)
	{
		var array = data.ToCharArray();
		Assert.Equal(data, array);
		Assert.True(new ReadOnlyStringBuffer(array).Equals(data));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString01)]
	[InlineData(TestString02)]
	[InlineData(TestString03)]
	[InlineData(TestString04)]
	[InlineData(TestString05)]
	[InlineData(TestString06)]
	#endregion
	public unsafe void Constructor_BytePointer(string data)
	{
		var byteCount = Encoding.Default.GetByteCount(data);

		fixed (byte* pointer = Encoding.Default.GetBytes(data))
		{
			Assert.Equal(data, Encoding.Default.GetString(pointer, byteCount));
			Assert.True(new ReadOnlyStringBuffer(pointer, byteCount).Equals(data));
		}
	}

	[Theory]
	#region Attributes
	[InlineData(TestString02)]
	[InlineData(TestString03)]
	[InlineData(TestString04)]
	[InlineData(TestString06)]
	#endregion
	public void Constructor_ByteArray(string data)
	{
		var array = Encoding.Default.GetBytes(data);
		Assert.True(new ReadOnlyStringBuffer(array).Equals(data));
	}

	[Theory]
	#region Attributes
	[InlineData(TestString02)]
	[InlineData(TestString03)]
	[InlineData(TestString04)]
	[InlineData(TestString06)]
	#endregion
	public void Constructor_ReadOnlySpan(string data)
	{
		ReadOnlySpan<char> span = data.AsSpan();
		Assert.True(new ReadOnlyStringBuffer(span).Equals(data));
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, 4, "Hey\r")]
	[InlineData(TestString01, 3, 2, "\r\n")]
	[InlineData(TestString01, 5, 1, "T")]
	[InlineData(TestString01, 7, 6, "is\rIs\u2029")]
	[InlineData(TestString01, 12, 8, "\u2029A Test ")]
	[InlineData(TestString01, 16, 4, "est ")]
	[InlineData(TestString01, 30, 3, "\r\n.")]
	[InlineData(TestString01, -3, 3, "\n.\u2028")]
	#endregion
	public void Slice_CharSpan(string data, int index, int length, string expectedSlice)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		var slice = buffer.Slice(index < 0 ? new(-index, true) : (Index)index, length);

		Assert.Equal(expectedSlice, new string(slice));
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, "Hey\r")]
	[InlineData(TestString01, 3, "\r\n")]
	[InlineData(TestString01, 5, "T")]
	[InlineData(TestString01, 7, "is\rIs\u2029")]
	[InlineData(TestString01, 12, "\u2029A Test ")]
	[InlineData(TestString01, 16, "est ")]
	[InlineData(TestString01, 30, "\r\n.")]
	[InlineData(TestString01, -3, "\n.\u2028")]
	#endregion
	public void TrySlice_CharSpan(string data, int index, string expectedSlice)
	{
		Span<char> span = stackalloc char[expectedSlice.Length];

		var buffer = new ReadOnlyStringBuffer(data);
		Assert.True(buffer.TrySlice(index < 0 ? new(-index, true) : (Index)index, span));

		Assert.Equal(expectedSlice, span);
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, 4, "Hey\r")]
	[InlineData(TestString01, 3, 5, "\r\n")]
	[InlineData(TestString01, 5, 6, "T")]
	[InlineData(TestString01, 7, 13, "is\rIs\u2029")]
	[InlineData(TestString01, 12, 20, "\u2029A Test ")]
	[InlineData(TestString01, 16, 20, "est ")]
	[InlineData(TestString01, 30, 33, "\r\n.")]
	[InlineData(TestString01, 31, 34, "\n.\u2028")]
	#endregion
	public void Slice_Range(string data, int startIndex, int endIndex, string expectedSlice)
	{
		Span<char> span = stackalloc char[expectedSlice.Length];

		var buffer = new ReadOnlyStringBuffer(data);
		Assert.True(buffer.TrySlice(startIndex..endIndex, span));

		Assert.Equal(expectedSlice, span);
	}

	[Fact]
	public void Slice_Range_FailsIfSpanTooSmall()
	{
		var buffer = new ReadOnlyStringBuffer(TestString05);
		Assert.False(buffer.TrySlice(0..7, stackalloc char[3]));
	}

	[Fact]
	public void ToString_SameAsInputData()
	{
		var buffer = new ReadOnlyStringBuffer(TestString02);
		Assert.Equal(TestString02, buffer.ToString());
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0)]
	[InlineData(TestString01, 3)]
	[InlineData(TestString01, 7)]
	[InlineData(TestString01, 8)]
	[InlineData(TestString01, 10)]
	[InlineData(TestString01, 12)]
	[InlineData(TestString01, 15)]
	[InlineData(TestString01, 21)]
	[InlineData(TestString01, 24)]
	[InlineData(TestString01, 26)]
	[InlineData(TestString01, 27)]
	[InlineData(TestString01, 30)]
	[InlineData(TestString01, 32)]
	[InlineData(TestString01, 33)]
	#endregion
	#region String ends with newline
	[InlineData(TestString02, 0)]
	[InlineData(TestString02, 2)]
	[InlineData(TestString02, 5)]
	[InlineData(TestString02, 9)]
	[InlineData(TestString02, 10)]
	[InlineData(TestString02, 12)]
	[InlineData(TestString02, 15)]
	[InlineData(TestString02, 19)]
	[InlineData(TestString02, 23)]
	[InlineData(TestString02, 27)]
	[InlineData(TestString02, 29)]
	[InlineData(TestString02, 31)]
	#endregion
	#region String with a lot of whitespace
	[InlineData(TestString03, 0)]
	[InlineData(TestString03, 2)]
	[InlineData(TestString03, 15)]
	[InlineData(TestString03, 29)]
	[InlineData(TestString03, 30)]
	[InlineData(TestString03, 42)]
	[InlineData(TestString03, 55)]
	[InlineData(TestString03, 69)]
	[InlineData(TestString03, 73)]
	#endregion
	#region String with a single line
	[InlineData(TestString05, 0)]
	[InlineData(TestString05, 2)]
	[InlineData(TestString05, 5)]
	[InlineData(TestString05, 9)]
	[InlineData(TestString06, 1)]
	[InlineData(TestString06, 4)]
	[InlineData(TestString06, 6)]
	[InlineData(TestString06, 8)]
	#endregion
	public void TryGetChar(string data, int index)
	{
		var buffer = new ReadOnlyStringBuffer(data);
		Assert.True(buffer.TryGetChar(index < 0 ? new(-index, true) : (Index)index, out char item));
		Assert.Equal(data[index], item);
	}

	[Fact]
	public void TryGetChar_FollowsSpecificEofConvention()
	{
		var buffer = new ReadOnlyStringBuffer(TestString05);
		Assert.False(buffer.TryGetChar(10, out char item));
		Assert.Equal('\0', item);
	}

	[Fact]
	public void TryGetChar_FailsIfEmpty()
	{
		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.False(buffer.TryGetChar(0, out char item));
		Assert.Equal('\0', item);
	}

	[Fact]
	public void TryGetChar_FailsIfOutOfRange()
	{
		var buffer = new ReadOnlyStringBuffer(TestString06);
		Assert.False(buffer.TryGetChar(14, out char item));
		Assert.Equal('\0', item);
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, "Hey")]
	[InlineData(TestString01, 1, "This")]
	[InlineData(TestString01, 2, "Is")]
	[InlineData(TestString01, 3, "A Test ")]
	[InlineData(TestString01, 4, " Method")]
	[InlineData(TestString01, 5, "")]
	[InlineData(TestString01, 6, ".")]
	[InlineData(TestString01, 7, "")] // eof
	#endregion
	#region String ends without newline
	[InlineData(TestString02, 0, "Hey")]
	[InlineData(TestString02, 1, "This")]
	[InlineData(TestString02, 2, "Is")]
	[InlineData(TestString02, 3, "A Test ")]
	[InlineData(TestString02, 4, " Method")]
	[InlineData(TestString02, 5, "")]
	[InlineData(TestString02, 6, ".")]
	[InlineData(TestString02, 7, "")] // eof
	#endregion
	#region Single-line strings
	[InlineData(TestString05, 0, "!rrrrrrrrr")]
	[InlineData(TestString05, 1, "")] // eof
	[InlineData(TestString06, 0, "r!!!!!!!!!")]
	[InlineData(TestString06, 1, "")] // eof
	#endregion
	public void TryGetLine(string data, int lineNumber, string expectedLine)
	{
		Span<char> span = stackalloc char[expectedLine.Length];

		var buffer = new ReadOnlyStringBuffer(data);
		Assert.True(buffer.TryGetLine(lineNumber, span));
		Assert.Equal(expectedLine, span);
	}

	[Fact]
	public void TryGetLine_FailsIfEmpty()
	{
		Span<char> span = [];

		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.False(buffer.TryGetLine(0, span));
		Assert.True(span.IsEmpty);
	}

	[Fact]
	public void TryGetLine_FailsIfOutOfRange()
	{
		Span<char> span = stackalloc char[10];

		var buffer = new ReadOnlyStringBuffer(TestString05);
		const string emptySequence = "\0\0\0\0\0\0\0\0\0\0";

		Assert.False(buffer.TryGetLine(2, span));
		Assert.Equal(emptySequence, span);

		Assert.False(buffer.TryGetLine(-1, span));
		Assert.Equal(emptySequence, span);
	}

	[Fact]
	public void TryGetLine_FalseIfSpanTooSmall()
	{
		Span<char> span = stackalloc char[5];

		var buffer = new ReadOnlyStringBuffer(TestString05);
		Assert.False(buffer.TryGetLine(0, span));
		Assert.Equal("\0\0\0\0\0", span);
	}

	[Theory]
	#region String ends with newline
	[InlineData(TestString01, 0, "Hey")]      // H in "Hey"
	[InlineData(TestString01, 3, "Hey")]      // CR in "Hey\r\n"
	[InlineData(TestString01, 4, "Hey")]      // LF in "Hey\r\n"
	[InlineData(TestString01, 5, "This")]     // T in "This"
	[InlineData(TestString01, 13, "A Test ")] // A in "A Test"
	[InlineData(TestString01, 15, "A Test ")] // T in "Test"
	[InlineData(TestString01, 22, " Method")] // M in "Method"
	[InlineData(TestString01, 28, " Method")] // First CR in "\r\n\r\n."
	[InlineData(TestString01, 29, " Method")] // First LF in "\r\n\r\n."
	[InlineData(TestString01, 30, "")]        // Second CR in "\r\n\r\n."
	[InlineData(TestString01, 31, "")]        // Second LF in "\r\n\r\n."
	[InlineData(TestString01, 32, ".")]       // Dot/point in Second LF in "\r\n\r\n."
	[InlineData(TestString01, 33, ".")]       // U2028 in ".\u2028"
	[InlineData(TestString01, 34, "")]        // End of file
	#endregion
	#region String ends without newline
	[InlineData(TestString02, 0, "Hey")]      // H in "Hey"
	[InlineData(TestString02, 3, "Hey")]      // CR in "Hey\r\n"
	[InlineData(TestString02, 4, "Hey")]      // LF in "Hey\r\n"
	[InlineData(TestString02, 5, "This")]     // T in "This"
	[InlineData(TestString02, 13, "A Test ")] // A in "A Test"
	[InlineData(TestString02, 15, "A Test ")] // T in "Test"
	[InlineData(TestString02, 22, " Method")] // M in "Method"
	[InlineData(TestString02, 28, " Method")] // First CR in "\r\n\r\n."
	[InlineData(TestString02, 29, " Method")] // First LF in "\r\n\r\n."
	[InlineData(TestString02, 30, "")]        // Second CR in "\r\n\r\n."
	[InlineData(TestString02, -2, "")]        // Second LF in "\r\n\r\n."
	[InlineData(TestString02, -1, ".")]       // Dot/point in Second LF in "\r\n\r\n."
	[InlineData(TestString02, 33, "")]        // End of file
	#endregion
	public void TryGetLineFromIndex(string data, int index, string expectedLine)
	{
		Span<char> span = stackalloc char[expectedLine.Length];

		var buffer = new ReadOnlyStringBuffer(data);
		Assert.True(buffer.TryGetLineFromIndex(index < 0 ? new(-index, true) : (Index)index, span));
		Assert.Equal(expectedLine, span.ToString());
	}

	[Fact]
	public void TryGetLineFromIndex_FailsIfEmpty()
	{
		Span<char> span = [];

		var buffer = new ReadOnlyStringBuffer(String.Empty);
		Assert.False(buffer.TryGetLineFromIndex(0, span));
		Assert.True(span.IsEmpty);
	}

	[Fact]
	public void TryGetLineFromIndex_FailsIfOutOfRange()
	{
		Span<char> span = stackalloc char[10];

		var buffer = new ReadOnlyStringBuffer(TestString05);
		const string emptySequence = "\0\0\0\0\0\0\0\0\0\0";

		Assert.False(buffer.TryGetLineFromIndex(12, span));
		Assert.Equal(emptySequence, span);

		Assert.False(buffer.TryGetLineFromIndex(^12, span));
		Assert.Equal(emptySequence, span);
	}

	[Fact]
	public void TryGetLineFromIndex_FalseIfSpanTooSmall()
	{
		Span<char> span = stackalloc char[5];

		var buffer = new ReadOnlyStringBuffer(TestString05);
		Assert.False(buffer.TryGetLineFromIndex(0, span));
		Assert.Equal("\0\0\0\0\0", span);
	}
}
