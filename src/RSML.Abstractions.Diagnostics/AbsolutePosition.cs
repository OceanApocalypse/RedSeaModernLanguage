using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OceanApocalypse.RSML.Abstractions.Diagnostics;

/// <summary>
/// An absolute reader position containing information on line and column numbers.
/// </summary>
public struct AbsolutePosition : IComparable<AbsolutePosition>, IEquatable<AbsolutePosition>, IEquatable<ValueTuple<int, int>>
{
    /// <summary>
    /// A default valid position initialized at line 1 and column 1.
    /// </summary>
    public static readonly AbsolutePosition Default = new(1, 1);

    /// <summary>
    /// The 1-based line number.
    /// </summary>
    public int Line
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 1;

    /// <summary>
    /// The 1-based column number.
    /// </summary>
    public int Column
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 1;

    /// <summary>
    /// Indicates whether the current instance is valid. A position is considered
    /// valid if the line is positive and the column is positive as well
    /// (meaning all data has been initialized).
    /// </summary>
    public readonly bool IsValid => Line > 0 && Column > 0;

    /// <summary>
    /// Initializes a new reader position.
    /// </summary>
    /// <param name="lineNumber">The current 1-based line number.</param>
    /// <param name="columnNumber">The current 1-based column number.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// At least one of the parameters was negative or zero.
    /// </exception>
    public AbsolutePosition(int lineNumber, int columnNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columnNumber);

        Line = lineNumber;
        Column = columnNumber;
    }

    /// <inheritdoc/>
    public readonly int CompareTo(AbsolutePosition other) => (Line, Column).CompareTo((other.Line, other.Column));

    /// <summary>
    /// Moves the current position to the start of the next line.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveToStartOfNextLine()
    {
        Line++;
        Column = 1;
    }

    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj switch
    {
        AbsolutePosition pos => Equals(pos),
        ValueTuple<long, int, int> tuple => Equals(tuple),
        _ => false
    };

    /// <inheritdoc/>
    public readonly bool Equals(AbsolutePosition other) => Line == other.Line && Column == other.Column;

    /// <summary>
    /// Indicates whether the current object is equal to a tuple representation
    /// of an object of the same type.
    /// </summary>
    /// <param name="tuple">The tuple representation.</param>
    /// <returns>True if equals.</returns>
    public readonly bool Equals(ValueTuple<int, int> tuple) => Line == tuple.Item1 && Column == tuple.Item2;

    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(Line, Column);

    /// <summary>
    /// Returns the tuple representation of the current instance.
    /// </summary>
    /// <returns>The tuple representation of the instance.</returns>
    public readonly (int Line, int Column) AsTuple() => new(Line, Column);

    /// <summary>
    /// Indicates whether the instance to the left is equals to the instance on the right. 
    /// </summary>
    /// <param name="left">One of the instances.</param>
    /// <param name="right">One of the instances.</param>
    /// <returns>True if equals.</returns>
    public static bool operator ==(AbsolutePosition left, AbsolutePosition right) => left.Equals(right);

    /// <summary>
    /// Indicates whether the instance to the left is different from the instance on the right. 
    /// </summary>
    /// <param name="left">One of the instances.</param>
    /// <param name="right">One of the instances.</param>
    /// <returns>True if different.</returns>
    public static bool operator !=(AbsolutePosition left, AbsolutePosition right) => !(left == right);

    /// <summary>
    /// Indicates whether the instance to the left has a lower offset than the instance on the right. 
    /// </summary>
    /// <param name="left">One of the instances.</param>
    /// <param name="right">One of the instances.</param>
    /// <returns>True if <paramref name="left"/>'s offset is less than <paramref name="right"/>'s.</returns>
    public static bool operator <(AbsolutePosition left, AbsolutePosition right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Indicates whether the instance to the left has a greater offset than the instance on the right. 
    /// </summary>
    /// <param name="left">One of the instances.</param>
    /// <param name="right">One of the instances.</param>
    /// <returns>True if <paramref name="left"/>'s offset is greater than <paramref name="right"/>'s.</returns>
    public static bool operator >(AbsolutePosition left, AbsolutePosition right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Indicates whether the instance to the left has a lower offset than the instance on the right,
    /// or if they're the same.
    /// </summary>
    /// <param name="left">One of the instances.</param>
    /// <param name="right">One of the instances.</param>
    /// <returns>True if <paramref name="left"/>'s offset is less than or equal to <paramref name="right"/>'s.</returns>
    public static bool operator <=(AbsolutePosition left, AbsolutePosition right) => left == right || left < right;

    /// <summary>
    /// Indicates whether the instance to the left has a greater offset than the instance on the right,
    /// or if they're the same.
    /// </summary>
    /// <param name="left">One of the instances.</param>
    /// <param name="right">One of the instances.</param>
    /// <returns>True if <paramref name="left"/>'s offset is greater than or equal to <paramref name="right"/>'s.</returns>
    public static bool operator >=(AbsolutePosition left, AbsolutePosition right) => left == right || left > right;

    /// <summary>
    /// Throws an <see cref="ArgumentException" /> if the given position is invalid.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <param name="paramName">The parameter name taken by the position.</param>
    /// <exception cref="ArgumentException">Position was not initialized yet (was invalid).</exception>
    public static void ThrowIfInvalid(AbsolutePosition position, string? paramName = null)
    {
        if (!position.IsValid)
            throw new ArgumentException("Position was not initialized yet (was therefore invalid).", paramName ?? nameof(position));
    }
}