using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// yield文。
	/// The yield statement.
    /// A yield statement transfers control to the caller expression and continues excecution until the program needs
    /// the code to execute next code.
    /// "yield" Expression ';' ;
    /// </summary>
    public class YieldStatement : Statement
    {
        /// <summary>
        /// yieldする式。
        /// The expression to be yielded.
        /// </summary>
        public Expression Expression{
            get => GetChildByRole(Roles.Expression);
            set => SetChildByRole(Roles.Expression, value);
		}

        public YieldStatement(Expression expression, TextLocation start, TextLocation end)
            : base(start, end)
		{
            Expression = expression;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitYieldStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitYieldStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitYieldStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as YieldStatement;
            return o != null && Expression.DoMatch(o.Expression, match);
        }

        #endregion
    }
}
