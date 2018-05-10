using System;
using System.Collections.Generic;
#if NETCOREAPP2_0
using System.Linq;
using Expresso.TypeSystem;
#endif
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    #if NETCOREAPP2_0
    using ExpressoKnownTypeReference = Expresso.TypeSystem.KnownTypeReference;
    using ExpressoKnownTypeCode = Expresso.TypeSystem.KnownTypeCode;
    #endif
    /// <summary>
    /// A simple type represents a user-defined type or a generic type that is composed of
    /// an identifier and type arguments.
    /// SimpleType nodes should not be associated with AstType nodes unless they refer to some aliases.
    /// The SimpleType.Null property can be used to indicate that there is some type.
    /// </summary>
    public class SimpleType : AstType
    {
        #region Null
        /// <summary>
        /// Simple.Null property can be used to indicate that there is some type.
        /// </summary>
        public new static readonly SimpleType Null = new NullSimpleType();

        sealed class NullSimpleType : SimpleType
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

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
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
        public string Identifier => IdentifierToken.Name;

        /// <summary>
        /// Gets the identifier as it is.
        /// </summary>
        public Identifier IdentifierToken{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public override Identifier IdentifierNode => IdentifierToken;

        /// <summary>
        /// Gets the type arguments.
        /// </summary>
        /// <value>The type arguments.</value>
        public AstNodeCollection<AstType> TypeArguments => GetChildrenByRole(Roles.TypeArgument);

        protected SimpleType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expresso.Ast.SimpleType"/> class
        /// for a simple user-defined type.
        /// </summary>
        /// <param name="identifier">The name of the type.</param>
        /// <param name="start">The start location this reference appears.</param>
        public SimpleType(string identifier, TextLocation start)
            : base(start, new TextLocation(start.Line, start.Column + identifier.Length))
        {
            IdentifierToken = MakeIdentifier(identifier, Modifiers.None, start);
        }

        public SimpleType(Identifier identifier, TextLocation start)
            : base(start, identifier.EndLocation)
        {
            IdentifierToken = identifier;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expresso.Ast.SimpleType"/> class
        /// with several type arguments.
        /// </summary>
        /// <param name="identifier">The name of the type.</param>
        /// <param name="typeArgs">Type arguments.</param>
        /// <param name="start">The start location this reference appears.</param>
        /// <param name="end">The location this reference ends at.</param>
        public SimpleType(string identifier, IEnumerable<AstType> typeArgs, TextLocation start, TextLocation end)
            : base(start, end)
        {
            IdentifierToken = MakeIdentifier(identifier, Modifiers.None, start);
            TypeArguments.AddRange(typeArgs);
        }

        public SimpleType(Identifier ident, IEnumerable<AstType> typeArgs, TextLocation start, TextLocation end)
            : base(start, end)
        {
            IdentifierToken = ident;
            TypeArguments.AddRange(typeArgs);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitSimpleType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSimpleType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitSimpleType(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            if(other is PlaceholderType)
                return true;
            
            var o = other as SimpleType;
            return o != null && IdentifierNode.DoMatch(o.IdentifierNode, match) && TypeArguments.DoMatch(o.TypeArguments, match);
        }

#if NETCOREAPP2_0
        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
        {
            if(Name == "tuple" && !TypeArguments.Any())
                return ExpressoKnownTypeReference.Get(ExpressoKnownTypeCode.Void);

            if(interningProvider == null)
                interningProvider = InterningProvider.Dummy;

            var type_args = TypeArguments.Select(ta => ta.ToTypeReference(lookupMode, interningProvider))
                                         .ToList();

            string identifier = interningProvider.Intern(Identifier);
            if(!type_args.Any() || string.IsNullOrEmpty(identifier)){
                // empty SimpleType is used for typeof(List<>)
                return SpecialType.UnboundTypeArgument;
            }

            var t = new SimpleTypeOrModuleReference(identifier, interningProvider.InternList(type_args), lookupMode);
            return interningProvider.Intern(t);
        }
#else
        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider)
        {
            throw new NotImplementedException();
        }
#endif
    }
}

