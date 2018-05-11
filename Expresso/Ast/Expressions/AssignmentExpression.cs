using System;
using System.Collections.Generic;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// 代入式。
    /// The assignment expression.
    /// Simple assignments like "x = 1" are represented as an assignment node.
    /// More complex assignments like "x = y = z = 3" are represented as composition of
    /// multiple assignment nodes, while simultaneous assignments like "x, y = 1, 2"
    /// are represented as an assignment node that has an <see cref="Expresso.Ast.SequenceExpression"/>
    /// on both the left-hand-side and right-hand-side.
    /// So, for example, "x = y = z = 3" is represented as follows:
    /// AssignmentExpression{
    ///     Left = AssginmentExpression{
    ///         Left = AssginmentExpression{
    ///             Left = PathExpression(Identifier("x")),
    ///             Right = PathExpression(Identifier("y"))
    ///         },
    ///         Right = PathExpression(Identifier("z"))
    ///     },
    ///     Right = LiteralExpression("3")
    /// }
    /// 
    /// while "x, y = 1, 2" is represented as:
    /// AssignmentExpression{
    ///     Left = SequenceExpression{
    ///         PathExpression(Identifier("x")),
    ///         PathExpression(Identifier("y"))
    ///     },
    ///     Right = SequenceExpression{
    ///         LiteralExpression(1),
    ///         LiteralExpression(2)
    ///     }
    /// }
    /// NOTE: The above table shows the structure in pseudo C# expressions for brevity.
    /// </summary>
    public class AssignmentExpression : Expression
    {
        public static readonly Role<Expression> LhsRole = BinaryExpression.LhsRole;
        public static readonly Role<Expression> RhsRole = BinaryExpression.RhsRole;

        public static readonly TokenRole AssignRole = new TokenRole("=", ExpressoTokenNode.Null);
        public static readonly TokenRole AddRole = new TokenRole("+=", ExpressoTokenNode.Null);
        public static readonly TokenRole SubtractRole = new TokenRole("-=", ExpressoTokenNode.Null);
        public static readonly TokenRole MultiplyRole = new TokenRole("*=", ExpressoTokenNode.Null);
        public static readonly TokenRole DivideRole = new TokenRole("/=", ExpressoTokenNode.Null);
        public static readonly TokenRole ModulusRole = new TokenRole("%=", ExpressoTokenNode.Null);
        public static readonly TokenRole PowerRole = new TokenRole("**=", ExpressoTokenNode.Null);
        public static readonly TokenRole ShiftLeftRole = new TokenRole("<<=", ExpressoTokenNode.Null);
        public static readonly TokenRole ShiftRightRole = new TokenRole(">>=", ExpressoTokenNode.Null);
        public static readonly TokenRole BitwiseAndRole = new TokenRole("&=", ExpressoTokenNode.Null);
        public static readonly TokenRole BitwiseOrRole = new TokenRole("|=", ExpressoTokenNode.Null);
        public static readonly TokenRole ExclusiveOrRole = new TokenRole("^=", ExpressoTokenNode.Null);

        /// <summary>
        /// 代入先の変数式。
        /// The target expression that will be bounded.
        /// </summary>
        public Expression Left{
            get => GetChildByRole(LhsRole);
            set => SetChildByRole(LhsRole, value);
		}

        /// <summary>
        /// 右辺値の式。
		/// The expression that will be assigned.
        /// </summary>
        public Expression Right{
            get => GetChildByRole(RhsRole);
            set => SetChildByRole(RhsRole, value);
		}

        public OperatorType Operator{
            get; set;
        }

        public ExpressoTokenNode OperatorToken => GetChildByRole(GetOperatorRole(Operator));

        public AssignmentExpression(Expression lhsExpr, Expression rhsExpr, OperatorType opType)
            : base(lhsExpr.StartLocation, rhsExpr.EndLocation)
		{
            Left = lhsExpr;
            Right = rhsExpr;
            Operator = opType;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitAssignment(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitAssignment(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitAssignment(this, data);
        }

        internal protected override bool DoMatch(AstNode other, Match match)
        {
            AssignmentExpression o = other as AssignmentExpression;
            return o != null && (o.Operator == OperatorType.Any || Operator == o.Operator)
                && Left.DoMatch(o.Left, match) && Right.DoMatch(o.Right, match);
        }

        public static TokenRole GetOperatorRole(OperatorType op)
        {
            switch(op){
            case OperatorType.Assign:
                return AssignRole;

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

            case OperatorType.BitwiseShiftLeft:
                return ShiftLeftRole;

            case OperatorType.BitwiseShiftRight:
                return ShiftRightRole;

            case OperatorType.BitwiseAnd:
                return BitwiseAndRole;

            case OperatorType.BitwiseOr:
                return BitwiseOrRole;

            case OperatorType.ExclusiveOr:
                return ExclusiveOrRole;

            default:
                throw new NotSupportedException("Invalid value for OperatorType");
            }
        }
    }
}
