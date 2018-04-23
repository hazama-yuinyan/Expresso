using System;
using System.Collections.Generic;
using System.Linq;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a `path`.
    /// A path, like the one in the filesystem, denotes the structural hierarchy to a specific item.
    /// In Expresso, an item can only be either a static field or a type.
    /// Identifier { "::" Identifier } ;
    /// </summary>
    public class PathExpression : Expression
    {
        #region Null
        public static new PathExpression Null = new NullPathExpression();
        sealed class NullPathExpression : PathExpression
        {
            public override bool IsNull{
                get{
                    return true;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitNullNode(this, data);
            }

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        /// <summary>
        /// Gets the path items.
        /// </summary>
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

        protected PathExpression()
        {
        }

        public PathExpression(Identifier ident)
            : base(ident.StartLocation, ident.EndLocation)
        {
            Items.Add(ident);
        }

        public PathExpression(IEnumerable<Identifier> paths)
            : base(paths.First().StartLocation, paths.Last().EndLocation)
        {
            Items.AddRange(paths);
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

