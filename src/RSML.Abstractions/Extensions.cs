using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace OceanApocalypse.RSML.Abstractions;

/// <summary>
/// Extension members for characters.
/// </summary>
public static class Extensions
{
	private const byte UpperLowerDiffBit = 0b_0010_0000; // 0x20, binary seems best suited for this tho ngl

	#region ASCII Characters
	private const byte Tab = 0x9;
	private const byte Lf = 0xA;
	private const byte Cr = 0xD;
	private const byte Space = 0x20;
	private const byte Exclamation = 0x21;
	private const byte And = 0x26;
	private const byte LessThan = 0x3C;
	private const byte GreaterThan = 0x3E;
	private const byte UppercaseA = 0x41;
	private const byte UppercaseZ = 0x5A;
	private const byte Pipe = 0x7C;
	#endregion

	extension(byte item)
	{
		/// <summary>
		/// Checks if the character in question represents an ASCII newline.
		/// </summary>
		/// <returns>True if ASCII newline.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAsciiNewline() => item is Lf or Cr;

		/// <summary>
		/// Checks if the character in question is ASCII punctuation. Used by RSML.
		/// </summary>
		/// <returns>True if ASCII punctuation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsRsmlPunctuation() => item is (>= LessThan and <= GreaterThan) or Exclamation or And or Pipe;

		/// <summary>
		/// Checks if the character in question is ASCII whitespace.
		/// </summary>
		/// <returns>True if ASCII punctuation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAsciiWhitespace() => item is Space or (>= Tab and <= Cr);

		/// <summary>
		/// Converts an ASCII letter to its uppercase form.
		/// </summary>
		/// <returns>The uppercase ASCII letter.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ToAsciiUpperInvariant() => (byte)(item & (~UpperLowerDiffBit));

		/// <summary>
		/// Checks if a given character is an ASCII letter.
		/// </summary>
		/// <returns>True if the character is an ASCII letter.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAsciiLetter() => ToAsciiUpperInvariant(item) is >= UppercaseA and <= UppercaseZ;

		/// <summary>
		/// Checks if a given character is an ASCII digit.
		/// </summary>
		/// <returns>True if the character is an ASCII digit.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAsciiDigit() => item is >= 48 and <= 57; // 48 is '0' and 57 is '9'

		/// <summary>
		/// Checks if a given character falls under the ASCII category.
		/// </summary>
		/// <returns>True if the character is ASCII.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAscii() => item is > 127;
	}

	extension(IImmutableList<string> strings)
	{
		/// <summary>
		/// Checks if an immutable array of strings contains a given character span.
		/// </summary>
		/// <param name="span">The span to check for.</param>
		/// <param name="comparisonType">The comparison mode to apply.</param>
		/// <returns>True if found.</returns>
		public bool Contains(ReadOnlySpan<char> span, StringComparison comparisonType = StringComparison.Ordinal)
		{
			foreach (string @string in strings)
			{
				if (!span.Equals(@string, comparisonType))
					return false;
			}

			return true;
		}
	}
}
