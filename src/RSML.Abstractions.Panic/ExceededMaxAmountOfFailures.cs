using System;

namespace OceanApocalypse.RSML.Abstractions.Panic;

/// <summary>
/// An exception that is thrown when a given operation exceeds the maximum
/// amount of internal failures it is allowed to endure.
/// This class cannot be inherited.
/// </summary>
public sealed class ExceededMaxAmountOfFailuresException : Exception
{
    /// <summary>
	/// Creates a new exception of this type with no message.
	/// </summary>
	public ExceededMaxAmountOfFailuresException() : base() { }

    /// <summary>
    /// Creates a new exception of this type with a custom error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ExceededMaxAmountOfFailuresException(string message) : base(message) { }

    /// <summary>
    /// Creates a new exception of this type with a custom error message and
    /// a reference to the exception that caused this panic.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that led to the panic.</param>
    public ExceededMaxAmountOfFailuresException(string? message, Exception innerException) : base(message, innerException) { }
}
