using System;
using System.Collections.Generic;

namespace Expresso.Ast
{
	/**
	 * While文はインスタンス生成時に全てのメンバーが決定しないので、メンバーは全部変更可能にしておく。
	 */

	/// <summary>
	/// While文.
	/// The While statement.
	/// </summary>
	/// <seealso cref="Node"/>
	/// <seealso cref="BreakableStatement"/>
    public class WhileStatement : Statement
	{
		/// <summary>
        /// 条件式。
		/// The condition.
        /// </summary>
        public Expression Condition{
            get{return GetChildByRole(Roles.Expression);}
        }

        /// <summary>
        /// 条件が真の間評価し続ける文(郡)。
		/// A statement or a block to be processed while the condition is true.
        /// </summary>
        public Statement Body{
            get{return GetChildByRole(Roles.EmbeddedStatement);}
        }

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

		public WhileStatement()
		{
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitWhileStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitWhileStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitWhileStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as WhileStatement;
            return o != null && Condition.DoMatch(o.Condition, match)
                && Body.DoMatch(o.Body, match);
        }

        #endregion
	}
}

