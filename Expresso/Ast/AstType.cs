using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;
using System.Collections.Generic;
using System.Diagnostics;


namespace Expresso.Ast
{
    /// <summary>
    /// A type reference in Expresso AST.
    /// An AstType is an ast node, while a TypeReference is an internal and public representation.
    /// The AstType.Null property can be used to represent the unit type, but we use an empty tuple type as the unit type.
    /// AstType nodes must be cloned when needed.
    /// </summary>
    public abstract class AstType : AstNode
    {
        #region Null
        public new static readonly AstType Null = new NullAstType();
        sealed class NullAstType : AstType
        {
            public override bool IsNull{
                get{
                    return true;
                }
            }

            public override string Name{
                get{
                    return null;
                }
            }

            public override Identifier IdentifierNode{
                get{
                    return Identifier.Null;
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

            internal protected override bool DoMatch(AstNode other, Match match)
            {
                #if DEBUG
                Debug.Write("<Null type>");
                #endif
                return other == null || other.IsNull;
            }

            #region implemented abstract members of AstType

            public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
            {
                return null;
            }

            #endregion
        }
        #endregion

        #region PatternPlaceholder
        public static implicit operator AstType(Pattern pattern)
        {
            return (pattern != null) ? new PatternPlaceholder(pattern) : null;
        }

        sealed class PatternPlaceholder : AstType, INode
        {
            readonly Pattern child;

            public PatternPlaceholder(Pattern child)
            {
                this.child = child;
            }

            public override NodeType NodeType{
                get{
                    return NodeType.Pattern;
                }
            }

            public override string Name{
                get{
                    return null;
                }
            }

            public override Identifier IdentifierNode{
                get{
                    return Identifier.Null;
                }
            }

            #region INode implementation

            public bool DoMatch(INode other, Match match)
            {
                return child.DoMatch(other, match);
            }

            public bool DoMatchCollection(ICSharpCode.NRefactory.Role role, INode pos, Match match, BacktrackingInfo backtrackingInfo)
            {
                return child.DoMatchCollection(role, pos, match, backtrackingInfo);
            }

            public override bool IsNull{
                get{
                    return false;
                }
            }

            #endregion

            #region implemented abstract members of AstNode

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitPatternPlaceholder(this, child);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitPatternPlaceholder(this, child);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitPatternPlaceholder(this, child, data);
            }

            protected internal override bool DoMatch(AstNode other, Match match)
            {
                return other is NullAstType;
            }

            #endregion

            #region implemented abstract members of AstType

            public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
            {
                return null;
            }

            #endregion
        }
        #endregion

        public override NodeType NodeType{
            get{
                return NodeType.TypeReference;
            }
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public abstract string Name{
            get;
        }

        /// <summary>
        /// Gets the type name as an identifier.
        /// </summary>
        public abstract Identifier IdentifierNode{
            get;
        }

        protected AstType()
        {
        }

        protected AstType(TextLocation start, TextLocation end)
            : base(start, end)
        {
        }

        public new AstType Clone()
        {
            return (AstType)base.Clone();
        }

        public new AstType MemberwiseClone()
        {
            return (AstType)base.MemberwiseClone();
        }

        /// <summary>
        /// Create an ITypeReference for this AstType.
        /// Uses the context (ancestors of this node) to determine the correct <see cref="NameLookupMode"/>.
        /// </summary>
        /// <remarks>
        /// The resulting type reference will read the context information from the
        /// <see cref="ITypeResolveContext"/>:
        /// For resolving type parameters, the CurrentTypeDefinition/CurrentMember is used.
        /// For resolving simple names, the current namespace and usings from the CurrentUsingScope
        /// (on CSharpTypeResolveContext only) is used.
        /// </remarks>
        public ITypeReference ToTypeReference(InterningProvider interningProvider = null)
        {
            return ToTypeReference(GetNameLookupMode(), interningProvider);
        }

        /// <summary>
        /// Create an ITypeReference for this AstType.
        /// </summary>
        /// <remarks>
        /// The resulting type reference will read the context information from the
        /// <see cref="ITypeResolveContext"/>:
        /// For resolving type parameters, the CurrentTypeDefinition/CurrentMember is used.
        /// For resolving simple names, the current module and imports from the CurrentModuleScope
        /// (on ExpressoTypeResolveContext only) is used.
        /// </remarks>
        public abstract ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null);

        /// <summary>
        /// Gets the name lookup mode from the context (looking at the ancestors of this <see cref="AstType"/>).
        /// </summary>
        public NameLookupMode GetNameLookupMode()
        {
            AstType outermost_type = this;
            while(outermost_type.Parent is AstType)
                outermost_type = (AstType)outermost_type.Parent;

            if(outermost_type.Parent is ImportDeclaration){
                return NameLookupMode.TypeInImportDeclaration;
            }else if(outermost_type.Role == Roles.BaseType){
                // Use BaseTypeReference for a type's base type, and for a constraint on a type.
                // Do not use it for a constraint on a method.
                if(outermost_type.Parent is TypeDeclaration /*|| (outermost_type.Parent is Constraint && outermost_type.Parent.Parent is TypeDeclaration)*/)
                    return NameLookupMode.BaseTypeReference;
            }
            return NameLookupMode.Type;
        }

        #region Factory methods
        public static PlaceholderType MakePlaceholderType(TextLocation loc = default(TextLocation))
        {
            return new PlaceholderType(loc);
        }

        public static PrimitiveType MakePrimitiveType(string name, TextLocation loc = default(TextLocation))
        {
            return new PrimitiveType(name, loc);
        }

        public static SimpleType MakeSimpleType(string name, TextLocation loc = default(TextLocation))
        {
            return new SimpleType(name, loc);
        }

        public static SimpleType MakeSimpleType(Identifier identifier, TextLocation loc = default(TextLocation))
        {
            return new SimpleType(identifier, loc);
        }

        public static SimpleType MakeSimpleType(string name, IEnumerable<AstType> typeArgs,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new SimpleType(name, typeArgs, start, end);
        }

        public static SimpleType MakeSimpleType(string name, TextLocation start, TextLocation end, params AstType[] typeArgs)
        {
            return new SimpleType(name, typeArgs, start, end);
        }

        public static ReferenceType MakeReferenceType(AstType baseType, TextLocation start = default(TextLocation))
        {
            return new ReferenceType(baseType, start);
        }

        public static MemberType MakeMemberType(AstType superType, SimpleType childType,
            TextLocation end = default(TextLocation))
        {
            return new MemberType(superType, childType, end);
        }

        public static FunctionType MakeFunctionType(string name, AstType returnType, IEnumerable<AstType> parameters,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new FunctionType(MakeIdentifier(name, Modifiers.None, start), returnType, parameters, start, end);
        }

        public static FunctionType MakeFunctionType(string name, AstType returnType, TextLocation start, TextLocation end,
            params AstType[] parameters)
        {
            return new FunctionType(MakeIdentifier(name, Modifiers.None, start), returnType, parameters, start, end);
        }

        public static ParameterType MakeParameterType(string name, TextLocation loc = default(TextLocation))
        {
            return new ParameterType(
                MakeIdentifier(name, Modifiers.None, loc)
            );
        }

        public static ParameterType MakeParameterType(Identifier identifier)
        {
            return new ParameterType(identifier);
        }
        #endregion
    }
}

