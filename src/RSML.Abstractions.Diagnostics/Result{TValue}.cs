using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OceanApocalypse.RSML.Abstractions.Diagnostics;

/// <summary>
/// An operation's result.
/// </summary>
/// <typeparam name="TValue">The type of the return value when successful.</typeparam>
public readonly struct Result<TValue> : IEquatable<Result<TValue>>
{
	/// <summary>
	/// The return value. Might be null or an arbitrary default if
	/// <see cref="IsSuccessful"/> is <c>false</c>.
	/// </summary>
	[MemberNotNullWhen(true, nameof(Value))]
	public readonly TValue? Value { get; }

	/// <summary>
	/// The diagnostic that serves as the error value when an error occurs.
	/// Might be null or an arbitrary default if <see cref="IsError"/> is <c>true</c>.
	/// </summary>
	public readonly Diagnostic Error { get; }

	/// <summary>
	/// Set to <c>true</c> when the operation is successful. Otherwise, it's set to <c>false</c>.
	/// It also indicates whether <see cref="Value"/> can be safely accessed or not (safe to access if
	/// <see cref="IsSuccessful"/> is <c>true</c>).
	/// </summary>
	/// <remarks>
	/// <see cref="IsSuccessful"/> and <see cref="IsError"/> are mutually exclusive conditions. When one is true,
	/// the other is false. This means you can use any of the two to evaluate whether the operation was successful
	/// and if it's safe to access <see cref="Value"/>.
	/// </remarks>
	[MemberNotNullWhen(true, nameof(Value))]
	public readonly bool IsSuccessful { get; }

	/// <summary>
	/// Set to <c>false</c> when the operation is successful. Otherwise, it's set to <c>true</c>.
	/// It also indicates whether <see cref="Value"/> can be safely accessed or not (safe to access if
	/// <see cref="IsError"/> is <c>false</c>).
	/// </summary>
	/// <remarks>
	/// <see cref="IsSuccessful"/> and <see cref="IsError"/> are mutually exclusive conditions. When one is true,
	/// the other is false. This means you can use any of the two to evaluate whether the operation was successful
	/// and if it's safe to access <see cref="Value"/>.
	/// </remarks>
	public readonly bool IsError => !IsSuccessful;

	internal Result(TValue value)
	{
		Value = value;
		Error = default;
		IsSuccessful = true;
	}

	internal Result(Diagnostic error)
	{
		Value = default;
		Error = error;
		IsSuccessful = false;
	}

	/// <summary>
	/// Checks if a given object is equal to the current instance.
	/// </summary>
	/// <param name="obj">The object to check against.</param>
	/// <returns>True if equals</returns>
	public override bool Equals(object? obj) =>
		obj is Result<TValue> result && Equals(result);

	/// <summary>
	/// Checks if a given result is equal to the current instance.
	/// </summary>
	/// <param name="other">The result to check against.</param>
	/// <returns>True if equals</returns>
	public bool Equals(Result<TValue> other) =>
		EqualityComparer<TValue?>.Default.Equals(Value, other.Value)
		&& Error.Equals(other.Error)
		&& IsSuccessful == other.IsSuccessful;

	/// <inheritdoc/>
	public override int GetHashCode() => unchecked(IsSuccessful ? EqualityComparer<TValue?>.Default.GetHashCode(Value) : Error.GetHashCode());

	/// <summary>
	/// Checks if two results are equals.
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <returns>True if equals</returns>
	public static bool operator ==(Result<TValue> left, Result<TValue> right) => left.Equals(right);

	/// <summary>
	/// Checks if two results are different.
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <returns>True if different</returns>
	public static bool operator !=(Result<TValue> left, Result<TValue> right) => !(left == right);
}
