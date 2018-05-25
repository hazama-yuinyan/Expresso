using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a destructuring pattern.
    /// A destructuring pattern tests the value against a type and if matched
    /// it decomposes the fields into child patterns.
    /// Unlike usual field access, the fields being decomposed must not be accessible from the scope
    /// that the pattern resides.
    /// TypePath '{' [ PatternConstruct ] '}'
    /// </summary>
    public class DestructuringPattern : PatternConstruct
    {
        /// <summary>
        /// Represents the path referring to the type name.
        /// </summary>
        public AstType TypePath{
            get => GetChildByRole(Roles.Type);
            set => SetChildByRole(Roles.Type, value);
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public AstNodeCollection<PatternConstruct> Items => GetChildrenByRole(Roles.Pattern);

        public DestructuringPattern(AstType typePath, IEnumerable<PatternConstruct> patterns)
            : base(typePath.StartLocation, patterns.Any() ? patterns.Last().EndLocation : typePath.EndLocation)
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

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as DestructuringPattern;
            return o != null && TypePath.DoMatch(o.TypePath, match) && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}

