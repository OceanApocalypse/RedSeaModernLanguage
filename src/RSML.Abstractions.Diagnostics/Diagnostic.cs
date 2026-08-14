using System;
using System.Diagnostics.CodeAnalysis;


namespace OceanApocalypse.RSML.Abstractions.Diagnostics;

/// <summary>
/// A diagnostic reported by RSML's API.
/// </summary>
public readonly struct Diagnostic : IFormattable, IEquatable<Diagnostic>
{
	/// <summary>
	/// The start location the error relates to (inclusive).
	/// </summary>
	public AbsolutePosition StartLocation { get; }

	/// <summary>
	/// The inclusive offset at which the range starts (inclusive).
	/// </summary>
	public long StartOffset { get; }

	/// <summary>
	/// The end location the error relates to (exclusive).
	/// </summary>
	public AbsolutePosition EndLocation { get; }

	/// <summary>
	/// The exclusive offset at which the range ends (exclusive).
	/// </summary>
	public long EndOffset { get; }

	/// <summary>
	/// The error's code. Contains information about the category of the error.
	/// </summary>
	public string Code { get; }

	/// <summary>
	/// Checks whether the error is internal (API error results, for example).
	/// </summary>
	public bool IsInternal => Code[1] == 'I';

	/// <summary>
	/// A brief error message detailing why it has happened.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// The error's severity.
	/// </summary>
	public Severity Severity { get; }

	/// <summary>Creates a new diagnostic with a basic error code.</summary>
	/// <param name="code">The error code.</param>
	public Diagnostic(string code)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ThrowIfInvalidErrorCode(code);

		Code = code;
		Message = "";
		Severity = Severity.None;
	}

	/// <summary>Creates a new diagnostic.</summary>
	/// <param name="code">The error code.</param>
	/// <param name="message">A brief error message detailing why it has happened.</param>
	public Diagnostic(string code, string message)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ThrowIfInvalidErrorCode(code);

		Code = code;
		Message = message;
		Severity = Severity.None;
	}

	/// <summary>Creates a new diagnostic.</summary>
	/// <param name="code">The error code.</param>
	/// <param name="severity">The error's severity.</param>
	public Diagnostic(string code, Severity severity)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ThrowIfInvalidErrorCode(code);

		Code = code;
		Message = "";
		Severity = severity;
	}

	/// <summary>Creates a new diagnostic.</summary>
	/// <param name="code">The error code.</param>
	/// <param name="message">A brief error message detailing why it has happened.</param>
	/// <param name="severity">The error's severity.</param>
	public Diagnostic(string code, string message, Severity severity)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ThrowIfInvalidErrorCode(code);

		Code = code;
		Message = message;
		Severity = severity;
	}

	/// <summary>Creates a new diagnostic.</summary>
	/// <param name="code">The error code.</param>
	/// <param name="spanStartOffset">The offset at which the range starts (inclusive).</param>
	/// <param name="spanStartDetails">More details on the range's start..</param>
	/// <param name="spanEndOffset">The offset at which the range ends (exclusive).</param>
	/// <param name="spanEndDetails">More details on the range's end.</param>
	/// <param name="message">A brief error message detailing why it has happened.</param>
	/// <param name="severity">The error's severity.</param>
	public Diagnostic(string code, long spanStartOffset, AbsolutePosition spanStartDetails, long spanEndOffset, AbsolutePosition spanEndDetails, string message, Severity severity)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ThrowIfInvalidErrorCode(code);

		Code = code;
		Message = message;
		Severity = severity;

		StartOffset = spanStartOffset;
		StartLocation = spanStartDetails;
		EndOffset = spanEndOffset;
		EndLocation = spanEndDetails;
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Diagnostic error && Equals(error);

	/// <inheritdoc/>
	public bool Equals(Diagnostic other) =>
		Message == other.Message && Code == other.Code && Severity == other.Severity && StartLocation.Equals(other.StartLocation) && EndLocation.Equals(other.EndLocation);

	/// <summary>
	/// Checks if two <see cref="Diagnostic"/>s are equal to each other.
	/// </summary>
	/// <returns>True if equals.</returns>
	public static bool operator ==(Diagnostic left, Diagnostic right) => left.Equals(right);

	/// <summary>
	/// Checks if two <see cref="Diagnostic"/>s are different from each other.
	/// </summary>
	/// <returns>True if different.</returns>
	public static bool operator !=(Diagnostic left, Diagnostic right) => !left.Equals(right);

	/// <inheritdoc/>
	public override int GetHashCode() => unchecked(HashCode.Combine(StartLocation, EndLocation, Code, Message, Severity));

	/// <summary>
	/// Returns a generic string representation of the current instance.
	/// </summary>
	/// <returns>The string representation.</returns>
	public override string ToString() => $"Diagnostic(Code={Code}, Start={StartLocation}, End={EndLocation}, Message={Message}, Severity={Severity})";

	/// <summary>
	/// Given a format, tries to return a string that uses said format as a basis for the representation.
	/// If it fails, it defaults to <see cref="ToString()"/>.
	/// </summary>
	/// <param name="format">The format. Available formats are: CTOR (constructor-like string), LOG (output-ready format) and JSON (struct as JSON).</param>
	/// <param name="formatProvider">Unused. Don't bother assigning it anything.</param>
	/// <returns>The string representation.</returns>
	public string ToString(string? format, IFormatProvider? formatProvider)
	{
		switch (format)
		{
			case "CTOR":
			case "I":
			case "INIT":
			case "NET":
				return $"new Diagnostic(\"{Code}\", \"{StartLocation}\", \"{EndLocation}\", \"{Message}\", {Severity})";

			case "LOG":
				string prefix = Severity switch
				{
					Severity.Message => "INFO ",
					Severity.Warning => "WARNING ",
					Severity.Error => "ERROR ",
					Severity.Critical => "CRITICAL ",
					_ => ""
				};

				if (StartLocation.Line == EndLocation.Line)
					return $"[{prefix}{Code}] @ L{StartLocation.Line + 1},C({StartLocation.Column + 1}..{EndLocation.Column + 1}) : {Message}";

				return $"[{prefix}{Code}] @ L({StartLocation.Line + 1}..{EndLocation.Line + 1}),C({StartLocation.Column + 1}..{EndLocation.Column + 1}) : {Message}";

			case "JSON":
				return $$"""
					{
						"errorCode": "{{Code}}",
						"range": [
							{
								"offset": {{StartOffset}},
								"line": {{StartLocation.Line}},
								"column": {{StartLocation.Column}}
							},
							{
								"offset": {{EndOffset}},
								"line": {{EndLocation.Line}},
								"column": {{EndLocation.Column}}
							}
						]
					}
					""";

			default:
				return ToString();
		}
	}

	private static void ThrowIfInvalidErrorCode(string code, string? paramName = null)
	{
		if (code.Length != 6 || code[0] != 'R' || code[1] is not 'I' and not 'L' and not 'S' and not 'P' and not '-' || !Char.IsAsciiDigit(code[2]) || !Char.IsAsciiDigit(code[3]) || !Char.IsAsciiDigit(code[4]) || !Char.IsAsciiDigit(code[5]))
			throw new ArgumentException("The error code is not in the correct format. Correct format in Regex is: R(-|I|L|P|S)\\d\\d\\d\\d", paramName ?? nameof(code));
	}
}
