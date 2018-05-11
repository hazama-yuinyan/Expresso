using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
	/// <summary>
	/// While文.
	/// The While statement.
    /// A while statement evaluates the expression and continues or drops the code afterward depending on
    /// whether the expression is evaluated to true or false.
    /// The other loop construct in Expresso.
    /// "while" Expression '{' Body '}' ;
	/// </summary>
    public class WhileStatement : Statement
	{
        #region NullObject
        public static new WhileStatement Null = new NullWhileStatement();

        sealed class NullWhileStatement : WhileStatement
        {
            public override bool IsNull => true;

            public override void AcceptWalker(IAstWalker walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data) => walker.VisitNullNode(this, data);

            protected internal override bool DoMatch (AstNode other, Match match) => other == null || other.IsNull;
        }
        #endregion
		/// <summary>
        /// 条件式。
		/// The condition which determines how long we'll continue executing the body statements.
        /// </summary>
        public Expression Condition{
            get => GetChildByRole(Roles.Expression);
            set => SetChildByRole(Roles.Expression, value);
        }

        /// <summary>
        /// 条件が真の間評価し続けるブロック文。
        /// The block to be processed while the condition is held true.
        /// </summary>
        public BlockStatement Body{
            get => GetChildByRole(Roles.Body);
            set => SetChildByRole(Roles.Body, value);
        }

        protected WhileStatement()
        {
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

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as WhileStatement;
            return o != null && Condition.DoMatch(o.Condition, match)
                && Body.DoMatch(o.Body, match);
        }

        #endregion
	}
}

