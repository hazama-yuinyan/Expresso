using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using System.Collections.Generic;
using System.Linq;


namespace Expresso.Ast
{
    /// <summary>
    /// Abstract implementation for entities.
    /// An <see cref="Expresso.Ast.EntityDeclaration"/> can be used to represent a member of either a class, or a module.
    /// </summary>
    public abstract class EntityDeclaration : AstNode
    {
        public static readonly Role<ExpressoModifierToken> ModifierRole = new Role<ExpressoModifierToken>("Modifier");

        public override NodeType NodeType{
            get{
                return NodeType.Member;
            }
        }

        public abstract SymbolKind SymbolKind{get;}

        public Modifiers Modifiers{
            get{return GetModifiers(this);}
            private set{SetModifiers(this, value);}
        }

        public bool HasModifier(Modifiers modifier)
        {
            return (Modifiers & modifier) == modifier;
        }

        public virtual string Name{
            get{return GetChildByRole(Roles.Identifier).Name;}
        }

        public virtual Identifier NameToken{
            get{return GetChildByRole(Roles.Identifier);}
        }

        public virtual AstType ReturnType{
            get{return GetChildByRole(Roles.Type);}
        }

        public ExpressoTokenNode SemicolonToken{
            get{return GetChildByRole(Roles.SemicolonToken);}
        }

        internal static Modifiers GetModifiers(AstNode node)
        {
            Modifiers m = 0;
            foreach(ExpressoModifierToken t in node.GetChildrenByRole(ModifierRole))
                m |= t.Modifier;

            return m;
        }

        internal static void SetModifiers(AstNode node, Modifiers newValue)
        {
            Modifiers old_value = GetModifiers(node);
            AstNode insertion_pos = node.FirstChild;/*node.GetChildrenByRole(AttributeRole).LastOrDefault();*/
            foreach(Modifiers m in ExpressoModifierToken.AllModifiers){
                if((m & newValue) != 0){
                    if((m & old_value) == 0){
                        // Modifier was added
                        var new_token = new ExpressoModifierToken(TextLocation.Empty, m);
                        node.InsertChildAfter(insertion_pos, new_token, ModifierRole);
                        insertion_pos = new_token;
                    }else{
                        // Modifier already exists
                        insertion_pos = node.GetChildrenByRole(ModifierRole).First(t => t.Modifier == m);
                    }
                }else{
                    if((m & old_value) != 0){
                        // Modifier was removed
                        node.GetChildrenByRole(ModifierRole).First(t => t.Modifier == m).Remove();
                    }
                }
            }
        }

        protected EntityDeclaration()
        {
        }

        protected EntityDeclaration(TextLocation start, TextLocation end)
            : base(start, end)
        {
        }

        #region Factory methods
        static internal FieldDeclaration MakeField(IEnumerable<Identifier> lhs, IEnumerable<Expression> rhs,
            Modifiers modifiers, TextLocation start, TextLocation end)
        {
            return new FieldDeclaration(lhs, rhs, modifiers, start, end);
        }

        static internal ParameterDeclaration MakeParameter(string name, AstType type, Expression option = null)
        {
            return new ParameterDeclaration(name, type, option);
        }

        static internal TypeDeclaration MakeClassDecl(string className, IEnumerable<AstType> bases,
            IEnumerable<EntityDeclaration> decls, Modifiers modifiers, TextLocation start, TextLocation end)
        {
            return new TypeDeclaration(className, bases, decls, modifiers, start, end);
        }

        static internal FunctionDeclaration MakeFunc(string name, IEnumerable<ParameterDeclaration> parameters, BlockStatement body,
            AstType returnType, Modifiers modifiers, TextLocation loc)
        {
            return new FunctionDeclaration(name, parameters, body, returnType, modifiers, loc);
        }

        /*static internal FunctionDeclaration MakeClosure(string name, IEnumerable<ParameterDeclaration> parameters, Block body,
            TypeAnnotation returnType, Stack<object> environ)
        {
            return new FunctionDeclaration(name, parameters.ToArray(), body, returnType, Flags.None, environ);
        }*/
        #endregion
    }
}

