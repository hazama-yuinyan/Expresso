using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Runtime;
using Expresso.Compiler;

namespace Expresso.Ast
{
	/// <summary>
	/// 式文。
	/// The expression statement.
    /// Expression ';'
	/// </summary>
    public class ExpressionStatement : Statement
	{
        /// <summary>
        /// 実行する式。
		/// The expression to be evaluated.
        /// </summary>
        public Expression Expression{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
		}

        public ExpressoTokenNode SemicolonToken{
            get{return GetChildByRole(Roles.SemicolonToken);}
        }

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

        public ExpressionStatement(Expression expr)
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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ExpressionStatement;
            return o != null && Expression.DoMatch(o.Expression);
        }

        #endregion
	}
}

