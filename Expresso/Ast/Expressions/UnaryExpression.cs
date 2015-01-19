using System;


namespace Expresso.Ast
{
	/// <summary>
	/// 単項演算。
	/// Reperesents an unary expression.
    /// UnaryOperator Expression ;
	/// </summary>
    public class UnaryExpression : Expression
	{
        public static readonly TokenRole NotRole = new TokenRole("!");
        public static readonly TokenRole ReferenceRole = new TokenRole("&");
        public static readonly TokenRole DereferenceRole = new TokenRole("*");

        readonly OperatorType ope;

		/// <summary>
		/// 演算子のタイプ。
		/// The type of the operation.
		/// </summary>
		public OperatorType Operator{
			get{return ope;}
		}

        public ExpressoTokenNode OperatorToken{
            get{return GetChildByRole(GetOperatorRole(ope));}
        }

		/// <summary>
		/// オペランド。
		/// The operand.
		/// </summary>
		public Expression Operand{
            get{return GetChildByRole(Roles.TargetExpression);}
            set{SetChildByRole(Roles.TargetExpression, value);}
		}

		public UnaryExpression(OperatorType opType, Expression target)
		{
			ope = opType;
            Operand = target;
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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
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
            case OperatorType.Dereference:
                return DereferenceRole;
            default:
                throw new NotSupportedException("Invalid value for UnaryOperatorType");
            }
        }
	}
}
