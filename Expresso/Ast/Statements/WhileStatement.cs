using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
	/// <summary>
	/// While文.
	/// The While statement.
    /// "while" Expression '{' Body '}' ;
	/// </summary>
    public class WhileStatement : Statement
	{
		/// <summary>
        /// 条件式。
		/// The condition.
        /// </summary>
        public Expression Condition{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        /// <summary>
        /// 条件が真の間評価し続けるブロック文。
        /// The block to be processed while the condition is true.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }

        public WhileStatement(Expression condition, BlockStatement block,
            TextLocation start)
            : base(start, block.EndLocation)
		{
            Condition = condition;
            Body = block;
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

