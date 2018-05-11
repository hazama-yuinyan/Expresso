using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the kind of the type.
    /// </summary>
    public enum ClassType
    {
        Class,
        Interface,
        Enum,
        NotType
    }

    /// <summary>
    /// 型定義。
    /// Represents a type declaration.
    /// A type declaration introduces a new type into the current scope.
    /// [ AttributeSection ] [ "export" ] ("class" | "interface" | "enum") Identifier [':' {PathExpression}]
    /// '{' {Decls} '}' ;
    /// </summary>
    public class TypeDeclaration : EntityDeclaration
    {
        public static readonly TokenRole ClassKeywordRole = new TokenRole("class", ExpressoTokenNode.Null);
        public static readonly TokenRole InterfaceKeywordRole = new TokenRole("interface", ExpressoTokenNode.Null);
        public static readonly TokenRole EnumKeywordRole = new TokenRole("enum", ExpressoTokenNode.Null);

        public override NodeType NodeType => NodeType.TypeDeclaration;

        public override SymbolKind SymbolKind => SymbolKind.TypeDefinition;

        public ExpressoTokenNode TypeKindToken{
            get{
                switch(type_kind){
                case ClassType.Class:
                    return GetChildByRole(ClassKeywordRole);

                case ClassType.Interface:
                    return GetChildByRole(InterfaceKeywordRole);

                case ClassType.Enum:
                    return GetChildByRole(EnumKeywordRole);

                default:
                    return ExpressoTokenNode.Null;
                }
            }
        }

        ClassType type_kind;

        public ClassType TypeKind{
            get{
                return type_kind;
            }

            set{
                ThrowIfFrozen();
                type_kind = value;
            }
        }

        public override string Name => NameToken.Name;

        public override Identifier NameToken => GetChildByRole(Roles.Identifier);

        public ExpressoTokenNode ColonToken => GetChildByRole(Roles.ColonToken);

        /// <summary>
        /// Represents the attribute.
        /// </summary>
        /// <value>The attribute.</value>
        public AttributeSection Attribute{
            get => GetChildByRole(AttributeRole);
            set => SetChildByRole(AttributeRole, value);
        }

        /// <summary>
        /// Gets all the base types.
        /// </summary>
        /// <value>The base types.</value>
        public AstNodeCollection<AstType> BaseTypes => GetChildrenByRole(Roles.BaseType);

        public ExpressoTokenNode LBrace => GetChildByRole(Roles.LBraceToken);

        /// <summary>
        /// Gets all the members defined in this declaration.
        /// </summary>
        /// <value>The members.</value>
        public AstNodeCollection<EntityDeclaration> Members => GetChildrenByRole(Roles.TypeMember);

        public ExpressoTokenNode RBrace => GetChildByRole(Roles.RBraceToken);

        public TypeDeclaration(ClassType classType, Identifier ident, IEnumerable<AstType> supers,
                               IEnumerable<EntityDeclaration> decls, AttributeSection attribute, Modifiers modifiers, TextLocation start, TextLocation end)
            : base(start, end)
        {
            TypeKind = classType;
            SetChildByRole(Roles.Identifier, ident);
            BaseTypes.AddRange(supers);
            Members.AddRange(decls);
            Attribute = attribute;
            SetModifiers(this, modifiers);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitTypeDeclaration(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitTypeDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitTypeDeclaration(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as TypeDeclaration;
            return o != null && TypeKind == o.TypeKind && BaseTypes.DoMatch(o.BaseTypes, match)
                && Members.DoMatch(o.Members, match);
        }

        #endregion
    }
}

