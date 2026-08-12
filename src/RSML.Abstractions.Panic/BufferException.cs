using System;
using System.IO;

namespace OceanApocalypse.RSML.Abstractions.Panic;

/// <summary>
/// An exception that occurs in buffer types.
/// </summary>
public class BufferException : IOException
{
	/// <summary>
	/// Creates a new buffer exception with no message.
	/// </summary>
	public BufferException() : base() { }

	/// <summary>
	/// Creates a new buffer exception with a custom error message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public BufferException(string message) : base(message) { }

	/// <summary>
	/// Creates a new buffer exception with a custom error message and a reference
	/// to the exception that caused this panic.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="innerException">The exception that led to the panic.</param>
	public BufferException(string message, Exception innerException) : base(message, innerException) { }
}
