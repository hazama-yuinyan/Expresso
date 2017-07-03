using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents an identifier pattern.
    /// An identifier pattern matches any identifier and optionally captures its value into a new variable.
    /// Identifier [ '@' PatternConstruct ] ;
    /// </summary>
    public class IdentifierPattern : PatternConstruct
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        public Identifier Identifier{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        /// <summary>
        /// With '@' mark followed, that identifier captures the immediately following pattern.
        /// </summary>
        public PatternConstruct InnerPattern{
            get{return GetChildByRole(Roles.Pattern);}
            set{SetChildByRole(Roles.Pattern, value);}
        }

        public IdentifierPattern(Identifier ident, PatternConstruct inner)
            : base(ident.StartLocation, (inner != null) ? inner.EndLocation : ident.EndLocation)
        {
            Identifier = ident;
            InnerPattern = inner;
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
            return o != null && Identifier.DoMatch(o.Identifier, match)
                && InnerPattern.DoMatch(o.InnerPattern, match);
        }

        #endregion
    }
}

