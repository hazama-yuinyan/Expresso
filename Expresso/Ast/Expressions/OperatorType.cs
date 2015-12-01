using System;


namespace Expresso.Ast
{
    /// <summary>
    /// 演算子のタイプ。
    /// Represents the kind of an operator.
    /// </summary>
    public enum OperatorType
    {
        None,
        /// <summary>
        /// Represents the assignment operator `=`
        /// </summary>
        Assign,
        /// <summary>
        /// Represents the reference operator `&`
        /// </summary>
        Reference,
        /// <summary>
        /// Represents the dereference operator `&`
        /// </summary>
        Dereference,
        /// <summary>
        /// Represents the add operator `+`
        /// </summary>
        Plus,
        /// <summary>
        /// Represents the subtract operator `-`
        /// </summary>
        Minus,
        /// <summary>
        /// Represents the multiply operator `*`
        /// </summary>
        Times,
        /// <summary>
        /// Represents the divide operator `/`
        /// </summary>
        Divide,
        /// <summary>
        /// Represents the exponentiation operator `**`
        /// </summary>
        Power,
        /// <summary>
        /// Represents the modulus operator `%`
        /// </summary>
        Modulus,
        /// <summary>
        /// Represents the less than operator `&lt;`
        /// </summary>
        LessThan,
        /// <summary>
        /// Represents the greater than operator `&gt;`
        /// </summary>
        GreaterThan,
        /// <summary>
        /// Represents the less than or equal operator `&lt;=`
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// Represents the greater than or equal operator `&gt;=`
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// Represents the equal operator `==`
        /// </summary>
        Equality,
        /// <summary>
        /// Represents the inequal operator `!=`
        /// </summary>
        InEquality,
        /// <summary>
        /// Represents the bitwise not operator `!`
        /// </summary>
        Not,
        /// <summary>
        /// Represents the and operator `&&`
        /// </summary>
        ConditionalAnd,
        /// <summary>
        /// Represents the or operator `||`
        /// </summary>
        ConditionalOr,
        /// <summary>
        /// Represents the bitwise and operator `&`
        /// </summary>
        BitwiseAnd,
        /// <summary>
        /// Represents the bitwise or operator `|`
        /// </summary>
        BitwiseOr,
        /// <summary>
        /// Represents the exclusive or operator `^`
        /// </summary>
        ExclusiveOr,
        /// <summary>
        /// Represents the left shift operator `&lt;&lt;`
        /// </summary>
        BitwiseShiftLeft,
        /// <summary>
        /// Represents the right shift operator `&gt;&gt;`
        /// </summary>
        BitwiseShiftRight,
        /// <summary>
        /// Represents the cast operator `as`
        /// </summary>
        As,
        /// <summary>
        /// Any operator(for pattern matching).
        /// </summary>
        Any
    }
}

