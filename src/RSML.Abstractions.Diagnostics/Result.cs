using System;

using OceanApocalypse.RSML.Abstractions.Diagnostics.ErrorCodes;

namespace OceanApocalypse.RSML.Abstractions.Diagnostics;

/// <summary>
/// A collection of factory methods for easier initialization of <see cref="Result{TValue}"/> objects.
/// </summary>
public static class Result
{
	/// <summary>
	/// Tries to run function <paramref name="check"/> and return its output,
	/// unless it fails, in which case the returned result is also a failure.
	/// </summary>
	/// <typeparam name="TValue">The type of value to return if successful</typeparam>
	/// <typeparam name="TException">
	/// The type of exception to check for.
	/// The check mode is not strict, polymorphism is checked.
	/// </typeparam>
	/// <param name="check">The function to try and run.</param>
	/// <exception cref="ArgumentNullException"><paramref name="check"/> is null.</exception>
	/// <returns>Either a successful result, with the function's return value, or a failure.</returns>
	public static Result<TValue> TryCatch<TValue, TException>(Func<TValue> check)
		where TException : Exception
	{
		ArgumentNullException.ThrowIfNull(check);

		try
		{
			return new(check());
		}
		catch (TException ex)
		{
			return FromException<TValue>(ex, InternalErrorCodes.UnhandledException);
		}
	}

	/// <summary>
	/// Returns a successful result with a given value.
	/// </summary>
	/// <typeparam name="TValue">The type of value.</typeparam>
	/// <param name="value">The value.</param>
	/// <returns>The successful result.</returns>
	public static Result<TValue> Success<TValue>(TValue value) => new(value);

	/// <summary>
	/// Returns a successful result with a <c>true</c> boolean.
	/// </summary>
	/// <returns>The successful result, with value set to <c>true</c>.</returns>
	public static Result<bool> Success() => new(true);

	/// <summary>
	/// Returns a failure ("bad" result) for a boolean type.
	/// </summary>
	/// <param name="error">The error.</param>
	/// <returns>The failure, as a boolean result.</returns>
	public static Result<bool> Failure(Diagnostic error) => new(error);

	/// <summary>
	/// Returns a failure ("bad" result) for a given type.
	/// </summary>
	/// <typeparam name="TValue">The type of value the result would normally contain.</typeparam>
	/// <param name="error">The error.</param>
	/// <returns>The failure.</returns>
	public static Result<TValue> Failure<TValue>(Diagnostic error) => new(error);

	/// <summary>
	/// Returns a failure ("bad" result) for a given type, given an <see cref="Exception"/>.
	/// </summary>
	/// <typeparam name="TValue">The type of value the result would normally contain.</typeparam>
	/// <param name="exception">The exception whose data to retrieve and use as failure data.</param>
	/// <param name="errorCode">A string error code for the failure. Leave null for a generic error code.</param>
	/// <returns>The failure.</returns>
	public static Result<TValue> FromException<TValue>(Exception? exception, string? errorCode) =>
		new(new Diagnostic(errorCode ?? InternalErrorCodes.GenericError, exception?.Message ?? "No message provided."));

	/// <summary>
	/// Checks if a given object is null and, if it is, returns a failure.
	/// If the object is not null, returns a successful result with a given <paramref name="value"/>.
	/// </summary>
	/// <typeparam name="TValue">The type of value the result will contain if <paramref name="check"/> is not null.</typeparam>
	/// <param name="check">The object to check if null.</param>
	/// <param name="value">The result value if <paramref name="check"/> is not null.</param>
	/// <returns>Either a successful result or a failure.</returns>
	public static Result<TValue> FromNullable<TValue>(object? check, TValue value) =>
		check is not null ? new(value) : new(new Diagnostic(ErrorCodes.InternalErrorCodes.NullCheckFailed, "Null-check failed: object is null."));
}
