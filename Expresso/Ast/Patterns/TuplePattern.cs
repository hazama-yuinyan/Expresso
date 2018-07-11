using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a tuple pattern.
    /// A tuple pattern matches a tuple, which consists of a pair of parentheses and values.
    /// '(' {ident ','} ')' ;
    /// </summary>
    public class TuplePattern : PatternConstruct
    {
        /// <summary>
        /// Gets all the child patterns.
        /// </summary>
        public AstNodeCollection<PatternConstruct> Patterns => GetChildrenByRole(Roles.Pattern);

        /// <summary>
        /// Represents the resolved type as <see cref="SimpleType"/>.
        /// Will be resolved in <see cref="Analysis.TypeChecker"/>
        /// </summary>
        /// <value>The type of the resolved.</value>
        public SimpleType ResolvedType{
            get => GetChildByRole(Roles.GenericType);
            set => SetChildByRole(Roles.GenericType, value);
        }

        public TuplePattern(IEnumerable<PatternConstruct> patterns)
            : base(patterns.First().StartLocation, patterns.Last().EndLocation)
        {
            Patterns.AddRange(patterns);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitTuplePattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitTuplePattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitTuplePattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as TuplePattern;
            return o != null && Patterns.DoMatch(o.Patterns, match);
        }

        #endregion
    }
}

