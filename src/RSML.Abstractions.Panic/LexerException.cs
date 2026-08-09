using System;

namespace OceanApocalypse.RSML.Abstractions.Panic;

/// <summary>
/// An exception that occurs in lexer types.
/// </summary>
public class LexerException : Exception
{
	/// <summary>
	/// Creates a new lexer exception with no message.
	/// </summary>
	public LexerException() : base() { }

	/// <summary>
	/// Creates a new lexer exception with a custom error message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public LexerException(string message) : base(message) { }

	/// <summary>
	/// Creates a new lexer exception with a custom error message and a reference
	/// to the exception that caused this panic.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="innerException">The exception that led to the panic.</param>
	public LexerException(string? message, Exception innerException) : base(message, innerException) { }
}
