﻿using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// カンマで区切られたリテラル式をあらわす。
	/// Reperesents a comma-separated list expression like "1,2,3,4".
	/// It could show up in a simultaneous assignment like "a, b = 1, 2".
    /// Expression ',' Expression { ',' Expression } ;
    /// </summary>
    /// <remarks>Note that this single node doesn't specify any semantics in Expresso.</remarks>
    public class SequenceExpression : Expression
    {
        /// <summary>
        /// リストのアイテム。
		/// The items of this list.
        /// </summary>
        public AstNodeCollection<Expression> Items => GetChildrenByRole(Roles.Expression);

        /// <summary>
        /// Gets the number of items.
        /// </summary>
        /// <value>The count.</value>
        public int Count => Items.Count;

        public SequenceExpression(IEnumerable<Expression> items)
            : base(items.First().StartLocation, items.Last().EndLocation)
		{
            if(items != null)
                Items.AddRange(items);
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitSequenceExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSequenceExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitSequenceExpression(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as SequenceExpression;
            return o != null && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}
