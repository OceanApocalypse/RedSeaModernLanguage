using System;
using System.Diagnostics.CodeAnalysis;


namespace OceanApocalypse.RSML.Abstractions.Diagnostics;

/// <summary>
/// A diagnostic reported by RSML's API.
/// </summary>
public readonly struct Diagnostic : IFormattable, IEquatable<Diagnostic>
{
	/// <summary>
	/// The start index the error relates to (inclusive).
	/// </summary>
	public (Index Index, int Line, int Column) Start { get; } = (0, 0, 0);

	/// <summary>
	/// The end index the error relates to (exclusive).
	/// </summary>
	public (Index Index, int Line, int Column) End { get; } = (0, 0, 0);

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
		ArgumentNullException.ThrowIfNullOrWhiteSpace(code);
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
		ArgumentNullException.ThrowIfNullOrWhiteSpace(code);
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
		ArgumentNullException.ThrowIfNullOrWhiteSpace(code);
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
		ArgumentNullException.ThrowIfNullOrWhiteSpace(code);
		ThrowIfInvalidErrorCode(code);

		Code = code;
		Message = message;
		Severity = severity;
	}

	/// <summary>Creates a new diagnostic.</summary>
	/// <param name="code">The error code.</param>
	/// <param name="spanStart">The inclusive start of the range.</param>
	/// <param name="spanEnd">The exclusive end of the range.</param>
	/// <param name="message">A brief error message detailing why it has happened.</param>
	/// <param name="severity">The error's severity.</param>
	public Diagnostic(string code, (Index idx, int line, int col) spanStart, (Index idx, int line, int col) spanEnd, string message, Severity severity)
	{
		ArgumentNullException.ThrowIfNullOrWhiteSpace(code);
		ThrowIfInvalidErrorCode(code);

		Code = code;
		Start = spanStart;
		End = spanEnd;
		Message = message;
		Severity = severity;
	}

	/// <inheritdoc/>
	public override bool Equals(
		[NotNullWhen(true)]
		object? obj
	) => obj is Diagnostic error && Equals(error);

	/// <inheritdoc/>
	public bool Equals(Diagnostic other) => Message == other.Message && Code == other.Code && Severity == other.Severity && Start.Equals(other.Start) && End.Equals(other.End);

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
	public override int GetHashCode() => unchecked(HashCode.Combine(Start, End, Code, Message, Severity));

	/// <summary>
	/// Returns a generic string representation of the current instance.
	/// </summary>
	/// <returns>The string representation.</returns>
	public override string ToString() => $"Diagnostic(Code={Code}, Start={Start}, End={End}, Message={Message}, Severity={Severity})";

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
				return $"new Diagnostic(\"{Code}\", \"{Start}\", \"{End}\", \"{Message}\", {Severity})";

			case "LOG":
				string prefix = Severity switch
				{
					Severity.Message => "INFO ",
					Severity.Warning => "WARNING ",
					Severity.Error => "ERROR ",
					Severity.Critical => "CRITICAL ",
					_ => ""
				};

				if (Start.Line == End.Line)
					return $"[{prefix}{Code}] @ L{Start.Line + 1},C({Start.Column + 1}..{End.Column + 1}) : {Message}";

				return $"[{prefix}{Code}] @ L({Start.Line + 1}..{End.Line + 1}),C({Start.Column + 1}..{End.Column + 1}) : {Message}";

			case "JSON":
				return $$"""
					{
						"errorCode": "{{Code}}",
						"range": [
							{
								"index": {
									"value": {{Start.Index.Value}},
									"isFromEnd": {{Start.Index.IsFromEnd}}
								},
								"line": {{Start.Line}},
								"column": {{Start.Column}}
							},
							{
								"index": {
									"value": {{End.Index.Value}},
									"isFromEnd": {{End.Index.IsFromEnd}}
								},
								"line": {{End.Line}},
								"column": {{End.Column}}
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
		if (code.Length != 6 || code[0] != 'R' || code[1] is not 'I' and not 'L' and not 'S' and not 'P' and not '-' || Char.IsAsciiDigit(code[2]) || Char.IsAsciiDigit(code[3]) || Char.IsAsciiDigit(code[4]) || Char.IsAsciiDigit(code[5]))
			throw new ArgumentException("The error code is not in the correct format. Correct format in Regex is: R(-|I|L|P|S)\\d\\d\\d\\d", paramName ?? nameof(code));
	}
}
