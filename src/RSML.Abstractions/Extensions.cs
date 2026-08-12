using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Text;

namespace OceanApocalypse.RSML.Abstractions;

/// <summary>
/// Extension members for characters.
/// </summary>
public static class Extensions
{
	private readonly static UTF8Encoding exceptionlessUtf8Encoding = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
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

	extension(ReadOnlySequence<byte> sequence)
	{
		/// <summary>
		/// Safely decodes a UTF-8 sequence to a UTF-16 character span.
		/// </summary>
		/// <param name="destination">The destination span.</param>
		/// <param name="encoding">The UTF-8 encoding to use.</param>
		/// <returns>The amount of characters written.</returns>
		public int SafelyDecodeToCharacterSpan(Span<char> destination, UTF8Encoding? encoding = null)
		{
			int totalCharsWritten = 0;
			Decoder decoder = (encoding ?? exceptionlessUtf8Encoding).GetDecoder();

			foreach (var segment in sequence)
			{
				ReadOnlySpan<byte> span = segment.Span;
				decoder.Convert(span, destination[totalCharsWritten..], false, out _, out int charsWritten, out _);
				totalCharsWritten += charsWritten;
			}

			decoder.Convert([], destination[totalCharsWritten..], true, out _, out int flushCharsWritten, out _);
			totalCharsWritten += flushCharsWritten;
			return totalCharsWritten;
		}
	}

	extension(FrozenSet<string> stringSet)
	{
		/// <summary>
		/// Checks if an immutable array of UTF-16 strings contains a given UTF-8 span.
		/// </summary>
		/// <param name="seq">The span to check for.</param>
		/// <param name="stackAllocThreshold">The threshold that, when exceeded, ensures the code falls back to using arrays to avoid overflows.</param>
		/// <returns>True if found.</returns>
		public bool ContainsUtf8(ReadOnlySequence<byte> seq, int stackAllocThreshold = 256)
		{
			if (stringSet.Count == 0 || seq.IsEmpty)
				return false; // we fail fast over here

			var lookup = stringSet.GetAlternateLookup<ReadOnlySpan<char>>(); // lookup spans, not strings to avoid heap allocs obviously
			long length = seq.Length;

			if (length <= stackAllocThreshold) // the seq is small, we can use a span directly :)
			{
				Span<char> charBuffer = stackalloc char[(int)length];
				int charsWritten = seq.SafelyDecodeToCharacterSpan(charBuffer, exceptionlessUtf8Encoding);
				return lookup.Contains(charBuffer[..charsWritten]); // make sure to slice to avoid reading garbage data broski
			}
			else
			{
				char[] pooledArray = ArrayPool<char>.Shared.Rent((int)length); // we poolin' over here (direct span would be stack overflow)

				try
				{
					Span<char> charBuffer = pooledArray.AsSpan(0, (int)length); // get a span out of it, dum dum (we limit to length because sometimes it might pool larger arrays)
					int charsWritten = seq.SafelyDecodeToCharacterSpan(charBuffer, exceptionlessUtf8Encoding);
					return lookup.Contains(charBuffer[..charsWritten]); // make sure to slice to avoid reading garbage data brosquito
				}
				finally
				{
					ArrayPool<char>.Shared.Return(pooledArray); // we return to avoid leaking
				}
			}
		}
	}
}
