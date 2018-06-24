using System;
using System.Diagnostics;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a type parameter in generic type declarations.
    /// </summary>
    public class ParameterType : AstType
    {
        #region Null
        public new static readonly ParameterType Null = new NullParameterType();

        sealed class NullParameterType : ParameterType
        {
            public override bool IsNull => true;

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

            internal protected override bool DoMatch(AstNode other, Match match)
            {
                return other == null || other.IsNull;
            }

            public override ITypeReference ToTypeReference(NameLookupMode lookupMode, ICSharpCode.NRefactory.TypeSystem.InterningProvider interningProvider)
            {
                return SpecialType.UnknownType;
            }
        }
        #endregion

        public override string Name => Identifier;

        /// <summary>
        /// Gets the identifier as string.
        /// </summary>
        /// <value>The identifier.</value>
        public string Identifier => IdentifierToken.Name;

        /// <summary>
        /// Gets or sets the identifier as it is.
        /// </summary>
        public Identifier IdentifierToken{
            get => GetChildByRole(Roles.Identifier);
            set => SetChildByRole(Roles.Identifier, value);
        }

        public override Identifier IdentifierNode => IdentifierToken;

        protected ParameterType()
        {
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

        protected internal override bool DoMatch(AstNode other, Match match)
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

