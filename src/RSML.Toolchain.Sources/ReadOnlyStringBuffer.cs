using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

using OceanApocalypse.RSML.Toolchain.Abstractions;
using OceanApocalypse.RSML.Toolchain.Abstractions.Cache;
using OceanApocalypse.RSML.Toolchain.Abstractions.Panic;
using OceanApocalypse.RSML.Toolchain.Abstractions.Sources;


namespace OceanApocalypse.RSML.Toolchain.Sources;

/// <summary>
/// A read-only buffer backed by a string. All operations opt for performance
/// primarily via the internal use of <see cref="ReadOnlySpan{Char}"/> over string allocations
/// and also via caching.
/// </summary>
public sealed class ReadOnlyStringBuffer : IBuffer, ISupportsCache
{
	private const int AverageCharactersPerLine = 40;
	private const int ExtraCharacterCapacity = 64;
	private bool isDisposed;

	private readonly List<int> lineStarts = [];

	private readonly List<int> precededByCrLf = [];

	private readonly string data;

	/// <inheritdoc/>
	public bool CacheExists { get; private set; }

	/// <inheritdoc/>
	public bool IsEmpty => Length == 0;

	/// <remarks>
	/// Always returns <c>true</c>, as <see cref="ReadOnlyStringBuffer"/> only
	/// supports read-only content (hence the name).
	/// </remarks> 
	/// <inheritdoc/>
	public bool IsReadOnly => true;

	/// <inheritdoc/>
	public int Length => data.Length;

	/// <remarks>
	/// <see cref="LineCount"/> automatically builds cache if
	/// no cached data exists. No <see cref="BuildCache()"/> calls
	/// are necessary.
	/// </remarks>
	/// <inheritdoc/>
	public int LineCount => RawLineCount - 1; // ignore the "fake" EOF line (convention)

	private int RawLineCount
	{
		get
		{
			ComputeLineStarts();

			return lineStarts.Count;
		}
	}

	/// <inheritdoc/>
	public ReadOnlySpan<char> this[Range range] => data.AsSpan()[range];

	/// <inheritdoc/>
	public char this[Index index] => data[index];

	/// <inheritdoc/>
	public char this[int index] => data[index];

	/// <summary>
	/// Initializes a new <see cref="ReadOnlyStringBuffer"/>
	/// with a string.
	/// </summary>
	/// <param name="content">The string that the buffer will wrap.</param>
	public ReadOnlyStringBuffer(string content) => data = content;

	/// <summary>
	/// Initializes a new <see cref="ReadOnlyStringBuffer"/>
	/// by allocating a string from a <see cref="ReadOnlySpan{Char}"/>.
	/// </summary>
	/// <param name="content">The span pointing to the string's data.</param>
	public ReadOnlyStringBuffer(ReadOnlySpan<char> content) => data = content.ToString();

	/// <summary>
	/// Initializes a new <see cref="ReadOnlyStringBuffer"/>
	/// with an array of characters.
	/// </summary>
	/// <param name="content">The array of characters to use for the buffer.</param>
	public ReadOnlyStringBuffer(char[] content) => data = new(content);

	/// <summary>
	/// Initializes a new <see cref="ReadOnlyStringBuffer"/>
	/// with an array of bytes and the encoding to use when decoding them.
	/// </summary>
	/// <param name="content">The array of bytes to use for the buffer.</param>
	/// <param name="encoding">
	/// The encoding to use when decoding <paramref name="content"/>.
	/// Use <c>null</c> for the <see cref="Encoding.Default"/> encoding.
	/// </param>
	public ReadOnlyStringBuffer(byte[] content, Encoding? encoding = null) => data = encoding?.GetString(content) ?? Encoding.Default.GetString(content);

	/// <summary>
	/// Initializes a new <see cref="ReadOnlyStringBuffer"/>
	/// with a pointer referencing an array of bytes and the encoding
	/// to use when decoding them.
	/// </summary>
	/// <param name="contentPtr">The pointer referecing the array of bytes to use for the buffer.</param>
	/// <param name="byteCount">The amount of bytes in the array referenced by <paramref name="contentPtr"/>.</param>
	/// <param name="encoding">
	/// The encoding to use when decoding <paramref name="contentPtr"/>.
	/// Use <c>null</c> for the <see cref="Encoding.Default"/> encoding.
	/// </param>
	/// <remarks>This method is not CLS-compliant due to the unsafe context and the use of pointers.</remarks>
	[CLSCompliant(false)]
	public unsafe ReadOnlyStringBuffer(byte* contentPtr, int byteCount, Encoding? encoding = null) =>
		data = (encoding ?? Encoding.Default).GetString(contentPtr, byteCount);

	/// <inheritdoc/>
	public void BuildCache() => ComputeLineStarts();

	/// <inheritdoc/>
	public void BuildCache(bool forceRebuild) => ComputeLineStarts(forceRebuild);

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method allows the EOF index as in-range. The convention is as follows:
	/// - If the index is EOF (<see cref="Length"/>), then the output is always 0 and <paramref name="isCrLf"/> is always <c>false</c>.
	/// - If the index is the last (<see cref="Length"/> - 1), then the output is always 0.
	/// :::
	/// 
	/// :::info[Value of 'isCrLf' parameter]
	/// <paramref name="isCrLf"/> is only <c>true</c> if all the following conditions are true:
	/// - The next line start counting from <paramref name="index"/> is preceded by a CRLF sequence.
	/// - <paramref name="index"/> does not point to the LF in the CRLF sequence.
	/// - <paramref name="index"/> does not point to EOF.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public int CountUntilEndOfLine(Index index, out bool isCrLf)
	{
		isCrLf = false;
		int offset = index.GetOffset(Length);

		ThrowIfEmpty();
		ThrowIfOutOfRange(offset, true);

		if (offset == Length)
			return 0; // consumed the entire buffer

		ComputeLineStarts();

		int lineSep = GetNextLineStartPosition(offset, out _);
		isCrLf = precededByCrLf.Contains(lineSep) && data[index] is not '\n'; // to us, CRLF is only when we're not standing on the LF

		if (isCrLf)
			lineSep--; // skip the extra line separator in the CRLF sequence

		if (!(IsLastLine(offset) && !data[^1].IsNewline())) // if we're not on the last line and it doesn't end with a newline then
		{
			lineSep--;
		}

		return lineSep - offset;
	}

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method allows the EOF index as in-range.
	/// If the index is EOF (<see cref="Length"/>), then the output is always 0.
	/// :::
	/// 
	/// :::tip[About the return value]
	/// The return value, when summed with <paramref name="index"/>, becomes the index of the first character that
	/// is not whitespace, counting from <paramref name="index"/>.
	/// The only exception is if the buffer has been consumed (you pass EOF index or there's no more characters that are
	/// not whitespace), meaning the return value, when summed with <paramref name="index"/> is the value of
	/// <see cref="Length"/>, which is also the EOF index.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public int CountUntilNotWhitespace(Index index)
	{
		ThrowIfEmpty();
		int offset = index.GetOffset(Length);
		ThrowIfOutOfRange(offset, true);

		if (offset == Length)
			return 0; // consumed the entire buffer

		var span = data.AsSpan(index);
		int count = 0;

		while (count < span.Length && Char.IsWhiteSpace(span[count]))
			count++;

		return count;
	}

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method allows the EOF index as in-range.
	/// If the index is EOF (<see cref="Length"/>), then the output is always 0.
	/// :::
	/// 
	/// :::tip[About the return value]
	/// The return value, when summed with <paramref name="index"/>, becomes the index of the first character that
	/// is whitespace, counting from <paramref name="index"/>.
	/// The only exception is if the buffer has been consumed (you pass EOF index or there's no more characters that are
	/// whitespace), meaning the return value, when summed with <paramref name="index"/> is the value of
	/// <see cref="Length"/>, which is also the EOF index.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public int CountUntilWhitespace(Index index)
	{
		ThrowIfEmpty();
		int offset = index.GetOffset(Length);
		ThrowIfOutOfRange(offset, true);

		if (offset == Length)
			return 0; // consumed the entire buffer

		var span = data.AsSpan(offset);
		int count = 0;

		while (count < span.Length && !Char.IsWhiteSpace(span[count]))
			count++;

		return count;
	}

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method allows the EOF index as in-range.
	/// If the index is EOF (<see cref="Length"/>), then the output is always 0.
	/// :::
	/// 
	/// :::tip[About the return value]
	/// The return value, when summed with <paramref name="index"/>, becomes the index of the first character that
	/// fails to verify the <paramref name="predicate"/>, counting from <paramref name="index"/>.
	/// The only exception is if the buffer has been consumed (you pass EOF index or there's no more characters that fail to verify
	/// the <paramref name="predicate"/>), meaning the return value, when summed with <paramref name="index"/> is the value of
	/// <see cref="Length"/>, which is also the EOF index.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public int CountWhile(Index index, Func<int, char, bool> predicate)
	{
		if (predicate is null)
			throw new ArgumentNullException(nameof(predicate), "The object is null.");

		ThrowIfEmpty();
		int offset = index.GetOffset(Length);
		ThrowIfOutOfRange(offset, true);

		if (offset == Length)
			return 0; // consumed the entire buffer

		var span = data.AsSpan(offset);
		int count = 0;

		while (count < span.Length && predicate(count, span[count]))
			count++;

		return count;
	}

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method follows EOF conventions.
	/// EOF is considered a 0-character sequence in line N, where N is <see cref="LineCount"/>.
	/// Keep in mind N does not point to an actual line (it's just a convention), as line numbers are 0-based
	/// (meaning the actual last line is located at N - 1).
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public int GetLengthOfLine(int lineNumber)
	{
		ThrowIfEmpty();
		ComputeLineStarts();
		ThrowIfLineNumberOutOfRange(lineNumber);

		if (lineNumber == RawLineCount - 1)
			return 0; // EOF means the line is empty

		int start = lineStarts[lineNumber];
		int end = lineStarts[lineNumber + 1];

		if (precededByCrLf.Contains(end))
			end--; // skip the extra line separator in the CRLF sequence

		if (!(lineNumber + 2 == RawLineCount && !data[^1].IsNewline()))
		{
			// if we're not on the last line and it doesn't end with newline then
			end--; // skip one more line separator
		}

		return end - start;
	}

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method follows EOF conventions.
	/// EOF is considered a 0-character sequence in line N, where N is <see cref="LineCount"/>.
	/// Keep in mind N does not point to an actual line (it's just a convention), as line numbers are 0-based
	/// (meaning the actual last line is located at N - 1).
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public int GetLengthOfLineFromIndex(Index index) => GetLengthOfLine(GetLineNumberFromIndex(index));

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method follows EOF conventions.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public int GetLineNumberFromIndex(Index index)
	{
		ThrowIfEmpty();
		int offset = index.GetOffset(Length);
		ThrowIfOutOfRange(offset, true);
		ComputeLineStarts();

		int lineSepIndex = GetPreviousOrCurrentLineStartPositionInLineStartList(offset);
		return lineSepIndex;
	}

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method follows EOF conventions.
	/// EOF is considered a 0-character sequence in line N, where N is <see cref="LineCount"/>.
	/// Keep in mind N does not point to an actual line (it's just a convention), as line numbers are 0-based
	/// (meaning the actual last line is located at N - 1).
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public ReadOnlySpan<char> GetLine(int lineNumber)
	{
		ThrowIfEmpty();
		ComputeLineStarts();
		ThrowIfLineNumberOutOfRange(lineNumber);

		if (lineNumber + 1 == RawLineCount)
			return String.Empty; // EOF means the line is empty

		int start = lineStarts[lineNumber];
		int length = GetLengthOfLine(lineNumber);

		return data.AsSpan(start, length);
	}

	/// <inheritdoc/>
	public ReadOnlySpan<char> GetLineFromIndex(Index index) => GetLine(GetLineNumberFromIndex(index));

	/// <remarks>
	/// :::warning[EOF Conventions]
	/// Unlike with other <see cref="ReadOnlyStringBuffer"/> methods, this one
	/// does not follow EOF conventions and, because of that, does not accept the 
	/// EOF index (index at <see cref="Length"/>), because it is not
	/// considered part of any slice.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public ReadOnlySpan<char> Slice(Index start, int length)
	{
		if (length < 0)
			throw new ArgumentOutOfRangeException(nameof(length), "The slice length must be positive.");

		int offset = start.GetOffset(Length);
		ThrowIfOutOfRange(offset, true, nameof(start));

		return data.AsSpan(offset, length);
	}

	/// <remarks>
	/// :::warning[EOF Conventions]
	/// Unlike with other <see cref="ReadOnlyStringBuffer"/> methods, this one
	/// does not follow EOF conventions and, because of that, does not accept the 
	/// EOF index (index at <see cref="Length"/>), because it is not
	/// considered part of any slice.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public ReadOnlySpan<char> Slice(Range range)
	{
		int startOffset = range.Start.GetOffset(Length);
		int endOffset = range.End.GetOffset(Length);

		ThrowIfOutOfRange(startOffset, true, nameof(range));
		ThrowIfOutOfRange(endOffset, true, nameof(range));

		return data.AsSpan()[range];
	}

	/// <remarks>
	/// :::warning[EOF Conventions]
	/// Unlike with other <see cref="ReadOnlyStringBuffer"/> methods, this one
	/// does not follow EOF conventions and, because of that, does not accept the 
	/// EOF index (index at <see cref="Length"/>), because it is not
	/// considered part of any slice.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public bool TrySlice(Index start, Span<char> slice) => data.AsSpan(start.GetOffset(Length), slice.Length).TryCopyTo(slice);

	/// <inheritdoc/>
	public bool TrySlice(Range range, Span<char> slice) => data.AsSpan()[range].TryCopyTo(slice);

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method follows the EOF convention where the EOF character
	/// is 0 (<c>'\0'</c>) and the return value is <c>false</c>, due to EOF
	/// not being an actual buffer location.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public bool TryGetChar(Index index, out char item)
	{
		item = '\0'; // default

		if (IsEmpty || IsOutOfRange(index.IsFromEnd ? Length + (-index.Value) : index.Value))
			return false;

		item = data[index];

		return true;
	}

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method follows EOF conventions.
	/// EOF is considered a 0-character sequence in line N, where N is <see cref="LineCount"/>.
	/// Keep in mind N does not point to an actual line (it's just a convention), as line numbers are 0-based
	/// (meaning the actual last line is located at N - 1).
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public bool TryGetLine(int lineNumber, Span<char> destination)
	{
		ComputeLineStarts();

		if (IsEmpty || lineNumber < 0 || lineNumber >= RawLineCount)
			return false;

		if (lineNumber + 1 == RawLineCount)
			return true; // EOF means the line is empty (and destination is by default empty)

		int start = lineStarts[lineNumber];
		int length = GetLengthOfLine(lineNumber);

		return data.AsSpan(start, length).TryCopyTo(destination);
	}

	/// <remarks>
	/// :::info[EOF Conventions]
	/// This method follows EOF conventions.
	/// EOF is considered a 0-character sequence in line N, where N is <see cref="LineCount"/>.
	/// Keep in mind N does not point to an actual line (it's just a convention), as line numbers are 0-based
	/// (meaning the actual last line is located at N - 1). If <paramref name="index" /> is EOF, the
	/// line will also be EOF.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public bool TryGetLineFromIndex(Index index, Span<char> destination)
	{
		if (IsEmpty || IsOutOfRange(index.IsFromEnd ? Length + (-index.Value) : index.Value, followEofConvention: true)) // avoids panic from GetLineNumberFromIndex
			return false;

		var lineNumber = GetLineNumberFromIndex(index);
		return TryGetLine(lineNumber, destination);
	}

	/// <remarks>
	/// :::warning[EOF Conventions]
	/// Unlike with other <see cref="ReadOnlyStringBuffer"/> methods, this one
	/// does not follow EOF conventions and, because of that, does not accept the 
	/// EOF index (index at <see cref="Length"/>), because it is not
	/// considered a location.
	/// :::
	/// </remarks>
	/// <inheritdoc/>
	public (Index Index, int Line, int Column) GetLocationDetails(Index index)
	{
		ThrowIfEmpty();
		ThrowIfOutOfRange(index.IsFromEnd ? Length + (-index.Value) : index.Value);

		if (index.Value == 0) // best "best" case = triple zero
			return (0, 0, 0);

		ComputeLineStarts();
		int lineNumber = GetLineNumberFromIndex(index);

		return new(index, lineNumber, index.GetOffset(Length) - lineStarts[lineNumber]);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc/>
	public override bool Equals(
		[NotNullWhen(true)]
		object? obj
	) => obj switch
	{
		string str => Equals(str),
		char[] charArray => Equals(charArray),
		IBuffer buffer => Equals(buffer),
		ReadOnlyMemory<char> readOnlyMemory => Equals(readOnlyMemory),
		null => false,
		_ => false
	};

	/// <summary>
	/// Checks if an array of characters is equal to the current instance.
	/// </summary>
	/// <param name="other">The array.</param>
	/// <returns>True if equals.</returns>
	public bool Equals(char[]? other) => other is not null && data.Equals(other.AsSpan(), StringComparison.Ordinal);

	/// <summary>
	/// Checks if another read-only buffer is equal to the current instance.
	/// </summary>
	/// <param name="other">The other read-only buffer.</param>
	/// <returns>True if equals.</returns>
	public bool Equals(IBuffer? other) => other is not null && data.Equals(other.ToString(), StringComparison.Ordinal);

	/// <summary>
	/// Checks if a read-only contiguous region of memory is equal to the current instance.
	/// </summary>
	/// <param name="other">The region of memory.</param>
	/// <returns>True if equals.</returns>
	public bool Equals(ReadOnlyMemory<char> other) => Length == other.Length && data.SequenceEqual(other.Span);

	/// <summary>
	/// Checks if a read-only contiguous region of memory is equal to the current instance.
	/// </summary>
	/// <param name="other">The region of memory.</param>
	/// <returns>True if equals.</returns>
	public bool Equals(ReadOnlySpan<char> other) => Length == other.Length && data.SequenceEqual(other);

	/// <summary>
	/// Checks if a string is equal to the current instance.
	/// </summary>
	/// <param name="other">The string.</param>
	/// <returns>True if equals.</returns>
	public bool Equals(string? other) => other is not null && Length == other.Length && data.Equals(other, StringComparison.Ordinal);

	/// <inheritdoc/>
	public override int GetHashCode() => unchecked(HashCode.Combine(data, lineStarts, precededByCrLf));

	/// <summary>
	/// Returns the buffer's content as a <see cref="String"/>.
	/// </summary>
	/// <returns>The buffer's content.</returns>
	public override string ToString() => data;

	/// <summary>
	/// Checks if two read-only string buffers are equals.
	/// </summary>
	/// <returns>True if equals.</returns>
	public static bool operator ==(ReadOnlyStringBuffer left, ReadOnlyStringBuffer right) =>
		EqualityComparer<ReadOnlyStringBuffer>.Default.Equals(left, right);

	/// <summary>
	/// Checks if two read-only string buffers are different.
	/// </summary>
	/// <returns>True if different.</returns>
	public static bool operator !=(ReadOnlyStringBuffer left, ReadOnlyStringBuffer right) => !(left == right);

	/// <summary>
	/// Disposes of both managed and unmanaged resources.
	/// </summary>
	/// <param name="disposing">When set to <c>false</c>, disposes of unmanaged resources only.</param>
	private void Dispose(bool disposing)
	{
		if (isDisposed)
			return;

		if (disposing)
		{
			lineStarts.Clear();
			precededByCrLf.Clear();
			CacheExists = false;
		}

		isDisposed = true;
	}

	private void ComputeLineStarts(bool forceCache = false)
	{
		if (CacheExists && !forceCache)
			return;

		var span = data.AsSpan();

		lineStarts.Clear();
		precededByCrLf.Clear();

		lineStarts.Capacity = Math.Max(lineStarts.Capacity, span.Length / AverageCharactersPerLine + ExtraCharacterCapacity); // just a rough guess
		precededByCrLf.Capacity = Math.Max(precededByCrLf.Capacity, span.Length / AverageCharactersPerLine + (OperatingSystem.IsWindows() ? ExtraCharacterCapacity : 0));

		/* the following line ensures that if the last line:
		 * ends with CR, LF, U2028 or U2029
		 * ends with CRLF
		 * does not end with any of the above
		 * 
		 * it will be counted as a line no matter the outcome of the previous condition
		 * this makes it more obvious from a human side like "bruv my string is abc\ndef two lines right"
		 * it looks normal that there are 2 lines but someone will go "acshua'y, that's erm 1 line :skull:"
		 * not with RSML's official buffers nah bro
		*/
		int lastIndex = span.EndsWith("\r\n") ? span.Length - 2 : span.Length - 1;
		int i = 0;

		lineStarts.Add(0); // 0 is by convention the start of line (and also the start of the buffer)

		while (i < lastIndex)
		{
			if (!span[i].IsNewline())
			{
				i++;
				continue;
			}

			bool isCrLf = i < lastIndex && span[i] == '\r' && span[i + 1] == '\n';
			int nextStart = i + (isCrLf ? 2 : 1);
			lineStarts.Add(nextStart);

			if (isCrLf)
				precededByCrLf.Add(nextStart);

			i = nextStart;
		}

		lineStarts.Add(Length); // add the EOF as the start of a line

		CacheExists = true;
	}

	/// <param name="insertionPoint">The insertion point of the next line start.</param>
	/// <param name="lsListIndex">The index, in <see cref="lineStarts"/> where the line start is.</param>
	/// <returns>The line start index, in <see cref="data"/>.</returns>
	private int GetNextLineStartFromInsertionPoint(int insertionPoint, out int lsListIndex)
	{
		if (insertionPoint >= 0)
		{
			// 1st Case: the used index (might have been index + 1 based on the method that called this)
			// points to an actual line start
			lsListIndex = insertionPoint;
			return lineStarts[insertionPoint];
		}

		int nextIndex = ~insertionPoint;

		if (nextIndex == lineStarts.Count)
		{
			// 2nd Case: the next line start is outside of the buffer (EOF convention)
			lsListIndex = nextIndex;
			return Length;
		}

		lsListIndex = nextIndex;

		return lineStarts[nextIndex]; // 3rd Case: we found the next line start
	}

	private int GetNextLineStartPosition(int index, out int lsListIndex) =>
		// we use index + 1 to skip to the next line start if we're already standing on one :)
		GetNextLineStartFromInsertionPoint(lineStarts.BinarySearch(index + 1), out lsListIndex);

	/// <param name="insertionPoint">The insertion point of the next line start.</param>
	/// <returns>The line start index in <see cref="lineStarts"/>, in <see cref="data"/>.</returns>
	private int GetPreviousLineStartInLineStartListFromInsertionPoint(int insertionPoint)
	{
		if (insertionPoint >= 0)
		{
			// 1st Case: the used index (might have been index + 1 based on the method that called this)
			// points to an actual line start
			return insertionPoint;
		}

		int previousIndex = ~insertionPoint;

		if (previousIndex == lineStarts.Count)
		{
			// 2nd Case: the next line start is outside of the buffer (EOF convention)
			// however we're gonna decrement one because otherwise all indexes from the last line that are after
			// start of said line suddenly become part of the EOF line
			// however this might also mean we're at last character
			return previousIndex;
		}

		return previousIndex == 0 ? 0 : previousIndex - 1; // 3rd Case: we found the previous line start
	}

	private int GetPreviousOrCurrentLineStartPositionInLineStartList(int index)
	{
		if (index > 0)
		{
			// when the index is greater than 0, we can safely get the previous line start without getting a big shitty error
			// we use 0 as the start following standard conventions
			return GetPreviousLineStartInLineStartListFromInsertionPoint(lineStarts.BinarySearch(index));
		}

		return 0;
	}

	private bool IsLastLine(int index) => RawLineCount == 1 || index >= lineStarts[^2] && index < lineStarts[^1];

	/// <summary>
	/// If <paramref name="followEofConvention"/> is set to <strong>'false'</strong>:
	/// <list type="bullet">True if index is greater than or equal to the length.</list>
	/// <list type="bullet">This prevents throwing <see cref="ArgumentOutOfRangeException"/>
	/// or <see cref="IndexOutOfRangeException"/>.</list>
	/// If <paramref name="followEofConvention"/> is set to <strong>'true'</strong>:
	/// <list type="bullet">True if index is greater than the length.</list>
	/// <list type="bullet">This does NOT prevent throwing <see cref="ArgumentOutOfRangeException"/>
	/// or <see cref="IndexOutOfRangeException"/>.</list>
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsOutOfRange(int index, bool followEofConvention = false) =>
		index < 0 || index > Length || (!followEofConvention && index == Length);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int NormalizeIndex(int index) => index < 0 ? index + Length : index;

	private void ThrowIfLineNumberOutOfRange(int lineNumber, string? paramName = null)
	{
		if (lineNumber < 0 || lineNumber >= RawLineCount)
		{
			throw new ArgumentOutOfRangeException(
				paramName ?? nameof(lineNumber),
				"The line number is negative or greather than the buffer's line count, meaning it doesn't point to either any valid character or EOF."
			);
		}
	}

	private void ThrowIfOutOfRange(int index, bool followEofConvention = false, string? paramName = null)
	{
		if (IsOutOfRange(index, followEofConvention))
		{
			throw new ArgumentOutOfRangeException(
				paramName ?? nameof(index),
				followEofConvention
					? "The index is negative or greather than the buffer's length, meaning it doesn't point to either any valid character or EOF."
					: "The index is negative, greater than or equal to the buffer's length, meaning it doesn't point to any valid character. EOF is not allowed."
			);
		}
	}

	private void ThrowIfEmpty()
	{
		if (IsEmpty)
			throw new BufferException("panic: The buffer is empty and, therefore, all indexes are out of range.");
	}
}
