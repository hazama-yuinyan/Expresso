using System;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using System.Collections.Generic;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// A simple type represents an identifier and type arguments.
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
        /// <param name="end">The location this reference ends at.</param>
        public SimpleType(string identifier, TextLocation start)
            : base(start, new TextLocation(start.Line, start.Column + identifier.Length))
        {
            AddChild(AstNode.MakeIdentifier(identifier, this), Roles.Identifier);
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
            IdentifierToken = AstNode.MakeIdentifier(identifier, this);
            foreach(var type in typeArgs)
                AddChild(type, Roles.TypeArgument);
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
            return o != null && MatchString(Identifier, o.Identifier)
                && TypeArguments.DoMatch(o.TypeArguments, match);
        }

        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider)
        {
            if(interningProvider == null)
                interningProvider = InterningProvider.Dummy;

            var type_args = 
                from ta in TypeArguments
                select ta.ToTypeReference(lookupMode, interningProvider);

            string identifier = interningProvider.Intern(Identifier);
            /*if(!type_args.Any() || string.IsNullOrEmpty(identifier)){
                // empty SimpleType is used for typeof(List<>)
                return SpecialType.UnboundTypeArgument;
            }*/

            return null; //TODO: implement it
        }
    }
}

