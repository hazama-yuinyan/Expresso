﻿using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
	/// <summary>
	/// 単項演算。
	/// Reperesents an unary expression.
    /// An unary expression takes one operand and performs some operation on it.
    /// UnaryOperator Expression ;
	/// </summary>
    public class UnaryExpression : Expression
	{
        public static readonly TokenRole NotRole = new TokenRole("!", ExpressoTokenNode.Null);
        public static readonly TokenRole ReferenceRole = new TokenRole("&", ExpressoTokenNode.Null);

        readonly OperatorType op;

		/// <summary>
		/// 演算子のタイプ。
		/// The type of the operation.
		/// </summary>
		public OperatorType Operator => op;

        public ExpressoTokenNode OperatorToken{
            get => GetChildByRole(GetOperatorRole(op));
            set => SetChildByRole(GetOperatorRole(op), value);
        }

		/// <summary>
		/// オペランド。
		/// The operand.
		/// </summary>
		public Expression Operand{
            get => GetChildByRole(Roles.TargetExpression);
            set => SetChildByRole(Roles.TargetExpression, value);
		}

        public UnaryExpression(OperatorType opType, Expression target, TextLocation loc)
            : base(loc, target.EndLocation)
		{
			op = opType;
            Operand = target;
            OperatorToken = new ExpressoTokenNode(TextLocation.Empty, GetOperatorRole(op));
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitUnaryExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitUnaryExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitUnaryExpression(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as UnaryExpression;
            return o != null && OperatorToken.DoMatch(o.OperatorToken, match)
                && Operand.DoMatch(o.Operand, match);
        }

        #endregion

        public static TokenRole GetOperatorRole(OperatorType op)
        {
            switch(op){
            case OperatorType.Plus:
                return Roles.PlusToken;

            case OperatorType.Minus:
                return Roles.MinusToken;

            case OperatorType.Not:
                return NotRole;

            case OperatorType.Reference:
                return ReferenceRole;

            default:
                throw new NotSupportedException("Invalid value for UnaryOperatorType");
            }
        }
	}
}
