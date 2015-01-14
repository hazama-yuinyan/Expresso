using System;
using ICSharpCode.NRefactory;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a tuple pattern.
    /// '(' {ident ','} ')'
    /// </summary>
    public class TuplePattern : PatternConstruct
    {
        public AstNodeCollection<PatternConstruct> Patterns{
            get{return GetChildrenByRole(Roles.Pattern);}
        }

        public TuplePattern(List<PatternConstruct> patterns)
        {
            foreach(var pattern in patterns)
                AddChild(pattern, Roles.Pattern);
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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as TuplePattern;
            return o != null && Patterns.DoMatch(o.Patterns, match);
        }

        #endregion
    }
}

