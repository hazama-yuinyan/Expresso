using System;
using System.Collections.Generic;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// 二項演算。
	/// Represents a binary expression.
    /// Expression BinaryOperator Expression ;
    /// </summary>
    public class BinaryExpression : Expression
    {
        public static readonly TokenRole BitwiseOrRole = new TokenRole("|", ExpressoTokenNode.Null);
        public static readonly TokenRole ConditionalAndRole = new TokenRole("&&", ExpressoTokenNode.Null);
        public static readonly TokenRole ConditionalOrRole = new TokenRole("||", ExpressoTokenNode.Null);
        public static readonly TokenRole ExclusiveOrRole = new TokenRole("^", ExpressoTokenNode.Null);
        public static readonly TokenRole GreaterThanRole = new TokenRole(">", ExpressoTokenNode.Null);
        public static readonly TokenRole GreaterThanOrEqualRole = new TokenRole(">=", ExpressoTokenNode.Null);
        public static readonly TokenRole EqualityRole = new TokenRole("==", ExpressoTokenNode.Null);
        public static readonly TokenRole InEqualityRole = new TokenRole("!=", ExpressoTokenNode.Null);
        public static readonly TokenRole LessThanRole = new TokenRole("<", ExpressoTokenNode.Null);
        public static readonly TokenRole LessThanOrEqualRole = new TokenRole("<=", ExpressoTokenNode.Null);
        public static readonly TokenRole MultiplyRole = new TokenRole("*", ExpressoTokenNode.Null);
        public static readonly TokenRole DivideRole = new TokenRole("/", ExpressoTokenNode.Null);
        public static readonly TokenRole ModulusRole = new TokenRole("%", ExpressoTokenNode.Null);
        public static readonly TokenRole PowerRole = new TokenRole("**", ExpressoTokenNode.Null);
        public static readonly TokenRole ShiftLeftRole = new TokenRole("<<", ExpressoTokenNode.Null);
        public static readonly TokenRole ShiftRightRole = new TokenRole(">>", ExpressoTokenNode.Null);

        public static readonly Role<Expression> LhsRole = new Role<Expression>("Lhs", Expression.Null);
        public static readonly Role<Expression> RhsRole = new Role<Expression>("Rhs", Expression.Null);

		readonly OperatorType op;

        /// <summary>
        /// 演算子のタイプ。
		/// The type of the operator.
        /// </summary>
        public OperatorType Operator => op;

        /// <summary>
        /// 左辺のオペランド。
		/// The left operand.
        /// </summary>
        public Expression Left{
            get => GetChildByRole(LhsRole);
            set => SetChildByRole(LhsRole, value);
		}

        public ExpressoTokenNode OperatorToken{
            get => GetChildByRole(GetOperatorRole(op));
            set => SetChildByRole(GetOperatorRole(op), value);
        }

        /// <summary>
        /// 右辺のオペランド。
		/// The right operand.
        /// </summary>
        public Expression Right{
            get => GetChildByRole(RhsRole);
            set => SetChildByRole(RhsRole, value);
		}

		public BinaryExpression(Expression left, Expression right, OperatorType opType)
            : base(left.StartLocation, right.EndLocation)
		{
			op = opType;
            OperatorToken = new ExpressoTokenNode(TextLocation.Empty, GetOperatorRole(op));
            Left = left;
            Right = right;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitBinaryExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitBinaryExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitBinaryExpression(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as BinaryExpression;
            return o != null && OperatorToken.DoMatch(o.OperatorToken, match)
                && Left.DoMatch(o.Left, match) && Right.DoMatch(o.Right, match);
        }

        #endregion

        public static TokenRole GetOperatorRole(OperatorType op)
        {
            switch(op){
            case OperatorType.BitwiseAnd:
                return Roles.AmpersandToken;

            case OperatorType.BitwiseOr:
                return BitwiseOrRole;

            case OperatorType.ConditionalAnd:
                return ConditionalAndRole;

            case OperatorType.ConditionalOr:
                return ConditionalOrRole;

            case OperatorType.ExclusiveOr:
                return ExclusiveOrRole;

            case OperatorType.GreaterThan:
                return GreaterThanRole;

            case OperatorType.GreaterThanOrEqual:
                return GreaterThanOrEqualRole;

            case OperatorType.Equality:
                return EqualityRole;

            case OperatorType.InEquality:
                return InEqualityRole;

            case OperatorType.LessThan:
                return LessThanRole;

            case OperatorType.LessThanOrEqual:
                return LessThanOrEqualRole;

            case OperatorType.Plus:
                return Roles.PlusToken;

            case OperatorType.Minus:
                return Roles.MinusToken;

            case OperatorType.Times:
                return MultiplyRole;

            case OperatorType.Divide:
                return DivideRole;

            case OperatorType.Modulus:
                return ModulusRole;

            case OperatorType.Power:
                return PowerRole;

            case OperatorType.BitwiseShiftLeft:
                return ShiftLeftRole;

            case OperatorType.BitwiseShiftRight:
                return ShiftRightRole;

            default:
                throw new NotSupportedException("Invalid value for BinaryOperatorType");
            }
        }
    }
}
