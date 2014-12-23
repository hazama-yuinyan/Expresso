using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Expresso.Compiler;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
    /// <summary>
    /// 二項演算。
	/// Represents a binary expression.
    /// </summary>
    public class BinaryExpression : Expression
    {
        public static readonly TokenRole BitwiseAndRole = new TokenRole("&");
        public static readonly TokenRole BitwiseOrRole = new TokenRole("|");
        public static readonly TokenRole ConditionalAndRole = new TokenRole("&&");
        public static readonly TokenRole ConditionalOrRole = new TokenRole("||");
        public static readonly TokenRole ExclusiveOrRole = new TokenRole("^");
        public static readonly TokenRole GreaterThanRole = new TokenRole(">");
        public static readonly TokenRole GreaterThanOrEqualRole = new TokenRole(">=");
        public static readonly TokenRole EqualityRole = new TokenRole("==");
        public static readonly TokenRole InEqualityRole = new TokenRole("!=");
        public static readonly TokenRole LessThanRole = new TokenRole("<");
        public static readonly TokenRole LessThanOrEqualRole = new TokenRole("<=");
        public static readonly TokenRole AddRole = new TokenRole("+");
        public static readonly TokenRole SubtractRole = new TokenRole("-");
        public static readonly TokenRole MultiplyRole = new TokenRole("*");
        public static readonly TokenRole DivideRole = new TokenRole("/");
        public static readonly TokenRole ModulusRole = new TokenRole("%");
        public static readonly TokenRole PowerRole = new TokenRole("**");
        public static readonly TokenRole ShiftLeftRole = new TokenRole("<<");
        public static readonly TokenRole ShiftRightRole = new TokenRole(">>");

        public static readonly Role<Expression> LhsRole = new Role<Expression>("Lhs", Expression.Null);
        public static readonly Role<Expression> RhsRole = new Role<Expression>("Rhs", Expression.Null);

		readonly OperatorType ope;

        /// <summary>
        /// 演算子のタイプ。
		/// The type of the operator.
        /// </summary>
        public OperatorType Operator{
			get{return ope;}
		}

        /// <summary>
        /// 左辺のオペランド。
		/// The left operand.
        /// </summary>
        public Expression Left{
            get{return GetChildByRole(LhsRole);}
		}

        public ExpressoTokenNode OperatorToken{
            get{return GetChildByRole(GetOperatorRole(ope));}
        }

        /// <summary>
        /// 右辺のオペランド。
		/// The right operand.
        /// </summary>
        public Expression Right{
            get{return GetChildByRole(RhsRole);}
		}

		public BinaryExpression(Expression left, Expression right, OperatorType opType)
		{
			ope = opType;
            AddChild(left, LhsRole);
            AddChild(right, RhsRole);
		}

        public override bool Equals(object obj)
        {
            var x = obj as BinaryExpression;

            if(x == null)
                return false;

            return this.ope == x.ope
                && this.Left.Equals(x.Left)
                && this.Right.Equals(x.Right);
        }

        public override int GetHashCode()
        {
            return this.ope.GetHashCode() ^ this.Left.GetHashCode() ^ this.Right.GetHashCode();
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

        public static TokenRole GetOperatorRole(OperatorType op)
        {
            switch(op){
            case OperatorType.BitwiseAnd:
                return BitwiseAndRole;
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
                return AddRole;
            case OperatorType.Minus:
                return SubtractRole;
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
