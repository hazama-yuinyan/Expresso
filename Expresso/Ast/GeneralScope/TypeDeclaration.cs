using System;


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
    /// "class" | "struct" | "interface" | "enum" TypeName [':' {BaseType}]
    /// '{' {Decls} '}'
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

        public override ICSharpCode.NRefactory.TypeSystem.SymbolKind SymbolKind{
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

        public TypeDeclaration()
        {
        }
    }
}

