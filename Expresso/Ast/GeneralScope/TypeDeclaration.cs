using System;

using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    public enum ClassType
    {
        Class,
        Struct,
        Interface,
        Enum
    }

    /// <summary>
    /// 型定義。
    /// Represents a type declaration.
    /// 
    /// ("class" | "struct" | "interface" | "enum") TypeName [':' {BaseType}]
    /// '{' {Decls} '}' ;
    /// </summary>
    public class TypeDeclaration : EntityDeclaration
    {
        public static readonly TokenRole ClassKeywordRole = new TokenRole("class");
        public static readonly TokenRole StructKeywordRole = new TokenRole("struct");
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

                case ClassType.Struct:
                    return GetChildByRole(StructKeywordRole);

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

        public ExpressoTokenNode ColonToken{
            get{return GetChildByRole(Roles.ColonToken);}
        }

        public AstNodeCollection<AstType> BaseTypes{
            get{return GetChildrenByRole(Roles.BaseType);}
        }

        public ExpressoTokenNode LBrace{
            get{return GetChildByRole(Roles.LBraceToken);}
        }

        public AstNodeCollection<EntityDeclaration> Members{
            get{return GetChildrenByRole(Roles.TypeMember);}
        }

        public ExpressoTokenNode RBrace{
            get{return GetChildByRole(Roles.RBraceToken);}
        }

        public TypeDeclaration()
        {
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

