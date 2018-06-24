using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a key-value pair as a <see cref="AstType"/>.
    /// </summary>
    public class KeyValueType : AstType
    {
        public static readonly Role<ParameterType> KeyTypeRole =
            new Role<ParameterType>("KeyType", ParameterType.Null);

        #region Null
        public new static readonly KeyValueType Null = new NullKeyValueType();

        sealed class NullKeyValueType : KeyValueType
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

        /// <summary>
        /// Represents the key type.
        /// </summary>
        /// <value>The type of the key.</value>
        public ParameterType KeyType{
            get => GetChildByRole(KeyTypeRole);
            set => SetChildByRole(KeyTypeRole, value);
        }

        /// <summary>
        /// Represents the value type.
        /// </summary>
        /// <value>The type of the value.</value>
        public AstType ValueType{
            get => GetChildByRole(Roles.Type);
            set => SetChildByRole(Roles.Type, value);
        }

        public override string Name => KeyType.Name;

        public override Identifier IdentifierNode => KeyType.IdentifierNode;

        protected KeyValueType()
        {
        }

        public KeyValueType(ParameterType keyType, AstType valueType)
            : base(keyType.StartLocation, valueType.EndLocation)
        {
            KeyType = keyType;
            ValueType = valueType;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitKeyValueType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitKeyValueType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitKeyValueType(this, data);
        }

        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
        {
            throw new NotImplementedException ();
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as KeyValueType;
            return o != null && KeyType.DoMatch(o.KeyType, match) && ValueType.DoMatch(o.ValueType, match);
        }
    }
}
