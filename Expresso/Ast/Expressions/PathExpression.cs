using System;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a `path`.
    /// A path, like the one in the filesystem, denotes the structural hierarchy to a specific item.
    /// In Expresso, an item can only be either a static field or a type.
    /// [ "::" ] Identifier { "::" Identifier } ;
    /// </summary>
    public class PathExpression : Expression
    {
        /// <summary>
        /// Gets the path items.
        /// </summary>
        /// <value>The items.</value>
        public AstNodeCollection<Identifier> Items{
            get{return GetChildrenByRole(Roles.Identifier);}
        }

        /// <summary>
        /// Just for convenience.
        /// Imagine situations where you want to put an identifier in places on which
        /// expressions are expected. In our AST system, an identifier can't be put in those places
        /// so we must wrap it in a PathExpression and put it on in the place instead.
        /// So this property can be used to retrieve the single identifier.
        /// </summary>
        /// <remarks>
        /// Note that this property doesn't check whether the node is a wrapped-up identifier.
        /// So you must be careful when you plan to use it.
        /// </remarks>
        public Identifier AsIdentifier{
            get{return Items.FirstOrNullObject();}
        }

        public PathExpression(Identifier ident)
        {
            AddChild(ident, Roles.Identifier);
        }

        public PathExpression(IEnumerable<Identifier> paths)
        {
            foreach(var item in paths)
                AddChild(item, Roles.Identifier);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitPathExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitPathExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitPathExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as PathExpression;
            return o != null && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}

