using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
	/// <summary>
	/// 式文。
	/// The expression statement.
    /// An expression statement evaluates an expression.
    /// Only those expressions that have side effects are considered to be meaningful
    /// when wrapped in an expression statement.
    /// Expression ';' ;
	/// </summary>
    public class ExpressionStatement : Statement
	{
        /// <summary>
        /// 実行する式。
		/// The expression to be evaluated.
        /// </summary>
        public Expression Expression{
            get => GetChildByRole(Roles.Expression);
            set => SetChildByRole(Roles.Expression, value);
		}

        public ExpressoTokenNode SemicolonToken => GetChildByRole(Roles.SemicolonToken);

        public ExpressionStatement(Expression expr, TextLocation start, TextLocation end)
            : base(start, end)
		{
            Expression = expr;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitExpressionStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitExpressionStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitExpressionStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ExpressionStatement;
            return o != null && Expression.DoMatch(o.Expression, match);
        }

        #endregion
	}
}

