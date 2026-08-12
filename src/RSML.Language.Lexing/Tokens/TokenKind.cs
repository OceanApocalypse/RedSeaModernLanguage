namespace OceanApocalypse.RSML.Language.Lexing.Tokens;

/// <summary>
/// Represents a specific kind of token.
/// </summary>
public enum TokenKind
{
	/// <summary>
	/// An unknown token kind.
	/// </summary>
	/// <remarks>
	/// :::note
	/// Unknown tokens usually lead to toolchain errors.  
	/// :::
	/// </remarks>
	Unknown,

	/// <summary>
	/// The EOF token kind. Represents the end of a source.
	/// </summary>
	Eof,

	/// <summary>
	/// The name of a variable, constant, function or type.
	/// </summary>
	Identifier,

	/// <summary>
	/// A numeric literal.
	/// </summary>
	NumericLiteral,

	/// <summary>
	/// A string literal.
	/// </summary>
	StringLiteral,

	/// <summary>
	/// A built-in identifier.
	/// </summary>
	StandardLibraryIdentifier,

	/// <summary>
	/// The return keyword. Stops execution of the current scope with a given value.
	/// </summary>
	ReturnKeyword,

	/// <summary>
	/// The if keyword. Conditionalizes a statement into running only if the condition is met.
	/// </summary>
	IfKeyword,

	/// <summary>
	/// The requires keyword. Indicates extensions the file depends on.
	/// </summary>
	RequiresKeyword,

	/// <summary>
	/// The end keyword. Ends the file.
	/// </summary>
	EndKeyword,

	/// <summary>
	/// The previous keyword. Modifies end into closing the previous region instead.
	/// </summary>
	PreviousModifier,

	/// <summary>
	/// The region keyword. Creates a conditionalized region.
	/// </summary>
	RegionKeyword,

	/// <summary>
	/// The let keyword. Declares and assigns a constant.
	/// </summary>
	LetKeyword,

	/// <summary>
	/// The mut keyword. Modifies let into creating a variable instead.
	/// </summary>
	MutableModifier,

	/// <summary>
	/// The fn keyword. Modifies let into creating a function instead.
	/// </summary>
	FunctionModifier,

	/// <summary>
	/// The exec keyword. Executes a function without you having to use discards.
	/// Treats every function as a void function.
	/// </summary>
	ExecKeyword,

	/// <summary>
	/// The type keyword. Creates a type.
	/// </summary>
	TypeKeyword,

	/// <summary>
	/// The as keyword.
	/// </summary>
	AsKeyword,

	/// <summary>
	/// The struct keyword. Used with type and as to create a struct type.
	/// </summary>
	StructKeyword,

	/// <summary>
	/// The assignment operator (=).
	/// </summary>
	AssignmentOperator,

	/// <summary>
	/// The equality operator (==).
	/// </summary>
	EqualToOperator,

	/// <summary>
	/// The inequality operator (!=).
	/// </summary>
	NotEqualToOperator,

	/// <summary>
	/// The greater-than operator (>).
	/// </summary>
	GreaterThanOperator,

	/// <summary>
	/// The less-than operator (&lt;).
	/// </summary>
	LessThanOperator,

	/// <summary>
	/// The greater-than-or-equal-to operator (>=).
	/// </summary>
	GreaterThanOrEqualToOperator,

	/// <summary>
	/// The less-than-or-equal-to operator (&lt;=).
	/// </summary>
	LessThanOrEqualToOperator,

	/// <summary>
	/// The colon (:).
	/// </summary>
	Colon,

	/// <summary>
	/// The comma (,).
	/// </summary>
	Comma,

	/// <summary>
	/// The semicolon (;).
	/// </summary>
	Semicolon,

	/// <summary>
	/// The plus sign, used for sum (+).
	/// </summary>
	Plus,

	/// <summary>
	/// The hyphen (minus sign), used for subtraction (-).
	/// </summary>
	Minus,

	/// <summary>
	/// The star, used for multiplication (*).
	/// </summary>
	Star,

	/// <summary>
	/// The slash used for division (/).
	/// </summary>
	Slash,

	/// <summary>
	/// The open brace ({).
	/// </summary>
	OpenBrace,

	/// <summary>
	/// The closed brace (}).
	/// </summary>
	ClosedBrace,

	/// <summary>
	/// The open parenthesis.
	/// </summary>
	OpenParenthesis,

	/// <summary>
	/// The closed parenthesis.
	/// </summary>
	ClosedParenthesis,

	/// <summary>
	/// The member access mark (<c>.</c>), which is a dot.
	/// </summary>
	MemberAccess,

	/// <summary>
	/// The NOT operator. It swaps the boolean value of whatever
	/// comes next.
	/// </summary>
	NotOperator,

	/// <summary>
	/// The logic AND operator. Returns <c>true</c> only if both the
	/// left and right sides evaluate to <c>true</c>.
	/// </summary>
	LogicAndOperator,

	/// <summary>
	/// The logic OR operator. Returns <c>true</c> if either left, right
	/// or both sides evaluate to <c>true</c>.
	/// </summary>
	LogicOrOperator,

	/// <summary>
	/// The at symbol (<c>@</c>). Reserved for future use.
	/// </summary>
	AtSymbol,

	/// <summary>
	/// A comment, including the # symbol.
	/// </summary>
	Comment
}
