using System;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// A type reference in Expresso AST.
    /// </summary>
    public abstract class AstType : AstNode
    {
        #region Null
        public static readonly AstType NullType = new NullAstType();
        sealed class NullAstType : AstType
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

            internal protected override bool DoMatch(AstNode other, Match match)
            {
                return other == null || other.IsNull;
            }
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

            #region INode implementation

            public bool DoMatch(INode other, Match match)
            {
                child.DoMatch(other, match);
            }

            public bool DoMatchCollection(ICSharpCode.NRefactory.Role role, INode pos, Match match, BacktrackingInfo backtrackingInfo)
            {
                return child.DoMatchCollection(role, pos, match);
            }

            public override bool IsNull{
                get{
                    return false;
                }
            }

            #endregion
        }
        #endregion

        public override NodeType NodeType{
            get{
                NodeType.TypeReference;
            }
        }

        public AstType()
        {
        }

        public new AstType Clone()
        {
            return (AstType)base.MemberwiseClone();
        }

        #region implemented abstract members of AstNode
        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitAstType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitAstType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitArgument(this, data);
        }
        #endregion

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
        /// For resolving simple names, the current namespace and usings from the CurrentUsingScope
        /// (on CSharpTypeResolveContext only) is used.
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
                if(outermost_type.Parent is TypeDeclaration || (outermost_type.Parent is Constraint && outermost_type.Parent.Parent is TypeDeclaration))
                    return NameLookupMode.BaseTypeReference;
            }
            return NameLookupMode.Type;
        }
    }
}

