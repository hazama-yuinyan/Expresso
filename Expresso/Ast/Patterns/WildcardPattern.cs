using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the wildcard pattern.
    /// A wildcard pattern matches any value.
    /// '_' ;
    /// </summary>
    public class WildcardPattern : PatternConstruct
    {
        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitWildcardPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitWildcardPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitWildcardPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            return other is WildcardPattern;
        }

        #endregion
    }
}

