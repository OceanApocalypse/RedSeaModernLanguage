using System;

namespace OceanApocalypse.RSML.Abstractions.Sources;

/// <summary>
/// Represents a buffer of characters.
/// </summary>
public interface IBuffer : IDisposable, IEquatable<IBuffer>, IEquatable<char[]?>, IEquatable<string?>, IEquatable<ReadOnlyMemory<char>>
{
	// todo: add methods that can mutate the buffer (coming to v3.0.0-prerelease2 ??)

	/// <summary>
	/// Whether the source is completely empty.
	/// </summary>
	bool IsEmpty => Length == 0;

	/// <summary>
	/// Whether the source can be mutated.
	/// </summary>
	bool IsReadOnly { get; }

	/// <summary>
	/// The length of the source.
	/// </summary>
	int Length { get; }

	/// <summary>
	/// The total amount of lines in the buffer.
	/// </summary>
	/// <remarks>
	/// Keep in mind lines might be empty.
	/// </remarks>
	int LineCount { get; }

	/// <summary>
	/// Gets a single item out of the buffer.
	/// </summary>
	/// <param name="index">The index of the item to retrieve.</param>
	/// <returns>The item.</returns>
	char this[int index] { get; }

	/// <summary>
	/// Gets a single item out of the buffer.
	/// </summary>
	/// <param name="index">The index of the item to retrieve.</param>
	/// <returns>The item.</returns>
	char this[Index index] { get; }

	/// <summary>
	/// Gets a span of items out of the buffer.
	/// </summary>
	/// <param name="range">The range to retrieve.</param>
	/// <returns>The items.</returns>
	ReadOnlySpan<char> this[Range range] { get; }

	/// <summary>
	/// Counts the amount of items until the next line separator in the buffer, relative to a given <paramref name="index"/>.
	/// Only line separators count - regular whitespace do not. CRLF counts as a single line separator, to avoid double counting.
	/// </summary>
	/// <param name="index">The index at which to start counting.</param>
	/// <param name="isCrLf">
	/// Whether the line separator at which the method stopped is the CR in a CRLF sequence. If true, the next item in the buffer is LF.
	/// </param>
	/// <returns>The index of the next line separator, relative to an <paramref name="index"/>.</returns>
	int CountUntilEndOfLine(Index index, out bool isCrLf);

	/// <summary>
	/// Counts the amount of items until the next non-whitespace item in the buffer, relative to a given <paramref name="index"/>.
	/// Line separators are included in the whitespace category.
	/// </summary>
	/// <param name="index">The index at which to start counting.</param>
	/// <returns>The index of the next non-whitespace item, relative to a <paramref name="index"/>.</returns>
	int CountUntilNotWhitespace(Index index);

	/// <summary>
	/// Counts the amount of items until the next whitespace item in the buffer, relative to a given <paramref name="index"/>.
	/// Line separators are included in the whitespace category.
	/// </summary>
	/// <param name="index">The index at which to start counting.</param>
	/// <returns>The index of the next whitespace item, relative to a <paramref name="index"/>.</returns>
	int CountUntilWhitespace(Index index);

	/// <summary>
	/// Counts the amount of items, starting from a given <paramref name="index"/>,
	/// while a <paramref name="predicate"/> returns <c>true</c>.
	/// </summary>
	/// <param name="index">
	/// The index at which to start counting; all indexes will also be given to the
	/// <paramref name="predicate"/> as an offset that when added to the index of the position
	/// equal the actual index.
	/// </param>
	/// <param name="predicate">
	/// A function that takes the current index (relative to <paramref name="index"/>),
	/// which is incremented every item, and the item associated with it. Execution stops when
	/// the predicate returns <c>false</c> or the index is out of bounds.
	/// </param>
	/// <returns>The amount of items counted.</returns>
	int CountWhile(Index index, Func<int, char, bool> predicate);

	/// <summary>
	/// Returns the length of a line given its 0-based line number.
	/// Line separators do not count towards the length.
	/// </summary>
	/// <param name="lineNumber">The 0-based line number.</param>
	/// <returns>The length of the line.</returns>
	int GetLengthOfLine(int lineNumber);

	/// <summary>
	/// Returns the length of a line given a 0-based index of one
	/// of its items.
	/// Line separators do not count towards the length.
	/// </summary>
	/// <param name="index">The 0-based index whose line is considered.</param>
	/// <returns>The length of the line.</returns>
	int GetLengthOfLineFromIndex(Index index);

	/// <summary>
	/// Given a 0-based line number, returns the matching line as an array of buffer items.
	/// </summary>
	/// <param name="lineNumber">The 0-based line number.</param>
	/// <returns>The line as an array of items.</returns>
	ReadOnlySpan<char> GetLine(int lineNumber);

	/// <summary>
	/// Tries to read the line that contains the item at <paramref name="index"/>.
	/// No end of line characters are added.
	/// </summary>
	/// <param name="index">The index at which to determine what the current line is.</param>
	/// <returns>The line, as an array of items.</returns>
	ReadOnlySpan<char> GetLineFromIndex(Index index);

	/// <summary>
	/// Determines the 0-based line number of the line that contains the item located at <paramref name="index"/>.
	/// </summary>
	/// <param name="index">The index whose parent line's number is to be returned.</param>
	/// <returns>The 0-based number of the line that contains item located at <paramref name="index"/>.</returns>
	int GetLineNumberFromIndex(Index index);

	/// <summary>
	/// Converts an index into a location.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>The location.</returns>
	(Index Index, int Line, int Column) GetLocationDetails(Index index);

	/// <summary>
	/// Slices a region of the buffer.
	/// </summary>
	/// <param name="start">The index of the first item in the slice.</param>
	/// <param name="length">The amount of items to slice starting at <paramref name="start"/>.</param>
	/// <returns>A slice, as an array of items.</returns>
	ReadOnlySpan<char> Slice(Index start, int length);

	/// <summary>
	/// Slices a region of the buffer into a performant span.
	/// </summary>
	/// <param name="start">The index of the first item in the slice.</param>
	/// <param name="slice">The span serving as the destination for the slice.</param>
	bool TrySlice(Index start, Span<char> slice);

	/// <summary>
	/// Slices a region of the buffer into a performant span.
	/// </summary>
	/// <param name="range">The range indicating what the slice is.</param>
	/// <param name="slice">The span serving as the destination for the slice.</param>
	bool TrySlice(Range range, Span<char> slice);

	/// <summary>
	/// Tries to return the item at the specified <paramref name="index"/>.
	/// </summary>
	/// <param name="index">The item's location.</param>
	/// <param name="item">The item.</param>
	/// <returns>False if the buffer is out of bounds or an exception occured.</returns>
	bool TryGetChar(Index index, out char item);

	/// <summary>
	/// Given a 0-based line number, assigns the exact line to a result buffer (<paramref name="destination"/>).
	/// No end of line characters are added.
	/// </summary>
	/// <param name="lineNumber">The 0-based line number.</param>
	/// <param name="destination">The destination buffer for the line.</param>
	/// <returns>True if successful.</returns>
	bool TryGetLine(int lineNumber, Span<char> destination);

	/// <summary>
	/// Tries to read the line that contains the item at <paramref name="index"/>.
	/// No end of line characters are added.
	/// </summary>
	/// <param name="index">The index at which to determine what the current line is.</param>
	/// <param name="destination">The destination span that will contain the line.</param>
	/// <returns>True if successful.</returns>
	bool TryGetLineFromIndex(Index index, Span<char> destination);
}
