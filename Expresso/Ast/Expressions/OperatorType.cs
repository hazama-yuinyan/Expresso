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
        Assign,
        Reference,
        Dereference,
        Plus,
        Minus,
        Times,
        Divide,
        Power,
        Modulus,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Equality,
        InEquality,
        Not,
        ConditionalAnd,
        ConditionalOr,
        BitwiseOr,
        BitwiseAnd,
        ExclusiveOr,
        BitwiseShiftLeft,
        BitwiseShiftRight,
        As,
        /// <summary>
        /// Any operator(for pattern matching).
        /// </summary>
        Any
    }
}

