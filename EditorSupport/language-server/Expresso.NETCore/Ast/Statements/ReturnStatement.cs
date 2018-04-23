using System.Collections.Generic;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
    /// <summary>
    /// リターン文。
	/// The Return statement.
    /// A return statement transfers control to the caller and additionaly gives a value to the caller.
    /// "return" Expression ;
    /// </summary>
    public class ReturnStatement : Statement
    {
        /// <summary>
        /// 戻り値の式。
		/// The expression generating the return value. It can be null if there is no return value.
        /// </summary>
        public Expression Expression{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
		}

		public ReturnStatement(Expression expression, TextLocation loc)
            : base(loc, new TextLocation(expression.EndLocation.Line, expression.EndLocation.Column + 1))
		{
            Expression = expression;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitReturnStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitReturnStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitReturnStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ReturnStatement;
            return o != null && Expression.DoMatch(o.Expression, match);
        }

        #endregion
    }
}
