﻿using System;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// カンマで区切られたリテラル式をあらわす。
	/// Reperesents a comma-separated list expression like "1,2,3,4".
	/// It could show up in a simultaneous assignment like "a, b = 1, 2"
    /// </summary>
    public class SequenceExpression : Expression
    {
        /// <summary>
        /// リストのアイテム。
		/// The items of this list.
        /// </summary>
        public AstNodeCollection<Expression> Items{
            get{return GetChildrenByRole(Roles.Expression);}
		}

		public int Count{
            get{return Items.Count;}
		}

        public override NodeType NodeType{
            get{return NodeType.Expression;}
        }

        public SequenceExpression(IEnumerable<Expression> items)
		{
            foreach(var item in items)
                AddChild(item, Roles.Expression);
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitSequence(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSequence(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitSequence(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as SequenceExpression;
            return o != null && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}
