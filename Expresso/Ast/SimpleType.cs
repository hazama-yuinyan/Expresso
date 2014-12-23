using System;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a primitive type.
    /// </summary>
    public class SimpleType : AstType
    {
        #region Null
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
                return walker.VisitNullNode(this);
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
        }

        public AstNodeCollection<AstType> TypeArguments{
            get{return GetChildrenByRole(Roles.TypeArgument);}
        }

        public SimpleType()
        {
        }

        public SimpleType(string identifier)
        {
            AddChild(AstNode.MakeIdentifier(identifier), Roles.Identifier);
        }

        public SimpleType(Identifier identifier)
        {
            AddChild(identifier, Roles.Identifier);
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
            var o = other as SimpleType;
            return o != null && MatchString(Identifier, o.Identifier) && TypeArguments.DoMatch(o.TypeArguments);
        }

        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider)
        {
            if(interningProvider == null)
                interningProvider = InterningProvider.Dummy;

            var type_args = 
                from ta in TypeArguments
                select ta.ToTypeReference(lookupMode, interningProvider);

            string identifier = interningProvider.Intern(Identifier);
            if(!type_args.Any() || string.IsNullOrEmpty(identifier)){
                // empty SimpleType is used for typeof(List<>)
                return SpecialType.UnboundTypeArgument;
            }


        }
    }
}

