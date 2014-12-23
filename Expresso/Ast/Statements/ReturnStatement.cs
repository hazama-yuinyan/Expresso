using System.Collections.Generic;

using Expresso.Compiler;

namespace Expresso.Ast
{
    /// <summary>
    /// リターン文。
	/// The Return statement.
    /// </summary>
    public class ReturnStatement : Statement
    {
        /// <summary>
        /// 戻り値の式。
		/// The expression generating the return value. It can be null if there is no return value.
        /// </summary>
        public Expression Expression{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression);}
		}

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

		public ReturnStatement(Expression expression)
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
            return o != null && Expression.DoMatch(o.Expression);
        }

        #endregion
    }
}
