using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
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
            public override bool IsNull{
                get{
                    return true;
                }
            }

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

        public override string Name{
            get{return Identifier;}
        }

        /// <summary>
        /// Gets the identifier as string.
        /// </summary>
        public string Identifier{
            get{return GetChildByRole(Roles.Identifier).Name;}
        }

        /// <summary>
        /// Gets the identifier as it is.
        /// </summary>
        public Identifier IdentifierToken{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public override Identifier IdentifierNode{
            get{return IdentifierToken;}
        }

        /// <summary>
        /// Gets the type arguments.
        /// </summary>
        /// <value>The type arguments.</value>
        public AstNodeCollection<AstType> TypeArguments{
            get{return GetChildrenByRole(Roles.TypeArgument);}
        }

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
            IdentifierToken = MakeIdentifier(identifier);
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
            IdentifierToken = MakeIdentifier(identifier);
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

        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider)
        {
            /*if(interningProvider == null)
                interningProvider = InterningProvider.Dummy;

            var type_args = 
                from ta in TypeArguments
                select ta.ToTypeReference(lookupMode, interningProvider);

            string identifier = interningProvider.Intern(Identifier);
            if(!type_args.Any() || string.IsNullOrEmpty(identifier)){
                // empty SimpleType is used for typeof(List<>)
                return SpecialType.UnboundTypeArgument;
            }*/

            return null; //TODO: implement it
        }
    }
}

