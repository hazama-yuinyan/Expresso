using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents an identifier pattern.
    /// Identifier [ "(-" Type ] ;
    /// </summary>
    public class IdentifierPattern : PatternConstruct
    {
        public Identifier Identifier{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public IdentifierPattern(Identifier ident)
        {
            Identifier = ident;
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitIdentifierPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitIdentifierPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitIdentifierPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as IdentifierPattern;
            return o != null && Identifier.DoMatch(o.Identifier, match);
        }

        #endregion
    }
}

