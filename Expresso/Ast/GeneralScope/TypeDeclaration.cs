using System;
using System.Collections.Generic;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    public enum ClassType
    {
        Class,
        Interface,
        Enum
    }

    /// <summary>
    /// 型定義。
    /// Represents a type declaration.
    /// 
    /// [ "export" ] ("class" | "interface" | "enum") Identifier [':' {PathExpression}]
    /// '{' {Decls} '}' ;
    /// </summary>
    public class TypeDeclaration : EntityDeclaration
    {
        public static readonly TokenRole ClassKeywordRole = new TokenRole("class");
        public static readonly TokenRole InterfaceKeywordRole = new TokenRole("interface");
        public static readonly TokenRole EnumKeywordRole = new TokenRole("enum");

        public override NodeType NodeType{
            get{
                return NodeType.TypeDeclaration;
            }
        }

        public override SymbolKind SymbolKind{
            get{
                return SymbolKind.TypeDefinition;
            }
        }

        public ExpressoTokenNode TypeKindToken{
            get{
                switch(class_type){
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

        ClassType class_type;

        public ClassType ClassType{
            get{
                return class_type;
            }

            set{
                ThrowIfFrozen();
                class_type = value;
            }
        }

        public override string Name{
            get{return NameToken.Name;}
        }

        public override Identifier NameToken{
            get{return GetChildByRole(Roles.Identifier);}
        }

        public ExpressoTokenNode ColonToken{
            get{return GetChildByRole(Roles.ColonToken);}
        }

        /// <summary>
        /// Gets all the base types.
        /// </summary>
        /// <value>The base types.</value>
        public AstNodeCollection<AstType> BaseTypes{
            get{return GetChildrenByRole(Roles.BaseType);}
        }

        public ExpressoTokenNode LBrace{
            get{return GetChildByRole(Roles.LBraceToken);}
        }

        /// <summary>
        /// Gets all the members defined in this declaration.
        /// </summary>
        /// <value>The members.</value>
        public AstNodeCollection<EntityDeclaration> Members{
            get{return GetChildrenByRole(Roles.TypeMember);}
        }

        public ExpressoTokenNode RBrace{
            get{return GetChildByRole(Roles.RBraceToken);}
        }

        public TypeDeclaration(string name, IEnumerable<AstType> supers,
            IEnumerable<EntityDeclaration> decls, Modifiers modifiers, TextLocation start, TextLocation end)
            : base(start, end)
        {
            AddChild(AstNode.MakeIdentifier(name), Roles.Identifier);

            foreach(var base_type in supers)
                AddChild(base_type, Roles.Type);

            foreach(var decl in decls)
                AddChild(decl, Roles.TypeMember);

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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as TypeDeclaration;
            return o != null && ClassType == o.ClassType && BaseTypes.DoMatch(o.BaseTypes, match)
                && Members.DoMatch(o.Members, match);
        }

        #endregion
    }
}

