using System;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a type parameter in generic type declarations.
    /// </summary>
    public class ParameterType : AstType
    {
        public override string Name{
            get{return Identifier;}
        }

        /// <summary>
        /// Gets the identifier as string.
        /// </summary>
        /// <value>The identifier.</value>
        public string Identifier{
            get{return GetChildByRole(Roles.Identifier).Name;}
        }

        /// <summary>
        /// Gets or sets the identifier as it is.
        /// </summary>
        public Identifier IdentifierToken{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public override Identifier IdentifierNode{
            get{return IdentifierToken;}
        }

        public ParameterType(Identifier identifier)
            : base(identifier.StartLocation, identifier.EndLocation)
        {
            IdentifierToken = identifier;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitParameterType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitParameterType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitParameterType(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ParameterType;
            if(o != null)
                return IdentifierToken.DoMatch(o.IdentifierToken, match);

            return other is AstType;
        }

        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider)
        {
            throw new NotImplementedException();
        }
    }
}

