using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a collection pattern.
    /// A collection pattern is the destructuring pattern for collections.
    /// </summary>
    public class CollectionPattern : PatternConstruct
    {
        /// <summary>
        /// Gets all the items.
        /// </summary>
        public AstNodeCollection<PatternConstruct> Items{
            get{return GetChildrenByRole(Roles.Pattern);}
        }

        /// <summary>
        /// Represents the collection type.
        /// Note that it doesn't contain any information about the element type.
        /// </summary>
        public AstType CollectionType{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        public CollectionPattern(IEnumerable<PatternConstruct> patterns, bool isVector)
        {
            CollectionType =
                isVector ? new PrimitiveType("vector", TextLocation.Empty) as AstType
                         : new SimpleType("array", new List<AstType>{
                               new PlaceholderType(TextLocation.Empty)
                         }, TextLocation.Empty, TextLocation.Empty) as AstType;

            foreach(var pattern in patterns)
                AddChild(pattern, Roles.Pattern);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitCollectionPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitCollectionPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitCollectionPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as CollectionPattern;
            return o != null && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}

