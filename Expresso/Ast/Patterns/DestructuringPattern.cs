using System;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a destructuring pattern.
    /// A destructuring pattern tests the value against a type and if matched
    /// it decompose the fields into child patterns.
    /// As with usual field access, the fields being decomposed must be accessible from the scope
    /// that the pattern resides.
    /// </summary>
    public class DestructuringPattern : PatternConstruct
    {
        /// <summary>
        /// Represents the path referring to the type name.
        /// </summary>
        public AstType TypePath{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public AstNodeCollection<PatternConstruct> Items{
            get{return GetChildrenByRole(Roles.Pattern);}
        }

        public DestructuringPattern(AstType typePath, IEnumerable<PatternConstruct> patterns)
        {
            TypePath = typePath;
            Items.AddRange(patterns);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitDestructuringPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitDestructuringPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitDestructuringPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as DestructuringPattern;
            return o != null && TypePath.DoMatch(o.TypePath, match) && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}

