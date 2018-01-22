using System;

using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
    /// <summary>
    /// 条件演算。
	/// The conditional expression.
    /// A conditional expression evaluates to either a value between question mark and colon or a value followed by colon.
    /// Expression '?' Expression ':' ConditionalExpression ;
    /// </summary>
    public class ConditionalExpression : Expression
    {
        public static readonly Role<Expression> TrueExpressionRole = new Role<Expression>("TrueExpr", Expression.Null);
        public static readonly Role<Expression> FalseExpressionRole = new Role<Expression>("FalseExpr", Expression.Null);

        /// <summary>
        /// 条件式。
		/// The condition expression to be tested.
        /// </summary>
        public Expression Condition{
            get{return GetChildByRole(Roles.TargetExpression);}
            set{SetChildByRole(Roles.TargetExpression, value);}
		}

        /// <summary>
        /// 条件が真の時に返す式。
		/// The expression to be evaluated when the condition is true.
        /// </summary>
        public Expression TrueExpression{
            get{return GetChildByRole(TrueExpressionRole);}
            set{SetChildByRole(TrueExpressionRole, value);}
		}

        /// <summary>
        /// 条件が偽の時に返す式。
		/// The expression to be evaluated when the condition is false.
        /// </summary>
        public Expression FalseExpression{
            get{return GetChildByRole(FalseExpressionRole);}
            set{SetChildByRole(FalseExpressionRole, value);}
		}

		public ConditionalExpression(Expression test, Expression trueExpr, Expression falseExpr)
		{
            Condition = test;
            TrueExpression = trueExpr;
            FalseExpression = falseExpr;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitConditionalExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitConditionalExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitConditionalExpression(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ConditionalExpression;
            return o != null && Condition.DoMatch(o.Condition, match)
                && TrueExpression.DoMatch(o.TrueExpression, match)
                && FalseExpression.DoMatch(o.FalseExpression, match);
        }

        #endregion
    }
}
