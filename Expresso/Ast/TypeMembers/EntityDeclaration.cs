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
        public static readonly Role<AttributeSection> AttributeRole = new Role<AttributeSection>("Attribute", AttributeSection.Null);
        public static readonly Role<ExpressoModifierToken> ModifierRole = new Role<ExpressoModifierToken>("Modifier");

        public override NodeType NodeType => NodeType.Member;

        public abstract SymbolKind SymbolKind{get;}

        public Modifiers Modifiers{
            get => GetModifiers(this);
            private set => SetModifiers(this, value);
        }

        public bool HasModifier(Modifiers modifier)
        {
            return (Modifiers & modifier) == modifier;
        }

        /// <summary>
        /// Gets the name as as string.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name => NameToken.Name;

        /// <summary>
        /// Gets the name token.
        /// </summary>
        /// <value>The name token.</value>
        public virtual Identifier NameToken => GetChildByRole(Roles.Identifier);

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>The type of the return.</value>
        public virtual AstType ReturnType => GetChildByRole(Roles.Type);

        public ExpressoTokenNode SemicolonToken => GetChildByRole(Roles.SemicolonToken);

        internal static Modifiers GetModifiers(AstNode node)
        {
            Modifiers m = Modifiers.None;
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
        public static FieldDeclaration MakeField(PatternWithType lhs, Expression rhs, Modifiers modifiers, AttributeSection attribute = null,
                                                 TextLocation start = default, TextLocation end = default)
        {
            return new FieldDeclaration(new []{lhs}, new []{rhs}, attribute, modifiers, start, end);
        }

        public static FieldDeclaration MakeField(IEnumerable<PatternWithType> lhs, IEnumerable<Expression> rhs,
                                                 Modifiers modifiers, AttributeSection attribute = null, TextLocation start = default, TextLocation end = default)
        {
            return new FieldDeclaration(lhs, rhs, attribute, modifiers, start, end);
        }

        public static ParameterDeclaration MakeParameter(string name, AstType type, Expression option = null, AttributeSection attribute = null, bool isVariadic = false,
                                                         TextLocation loc = default)
        {
            return new ParameterDeclaration(MakeIdentifier(name, type), option ?? Expression.Null, isVariadic, attribute, loc);
        }

        public static ParameterDeclaration MakeParameter(Identifier identifier, Expression option = null, AttributeSection attribute = null, bool isVariadic = false,
                                                         TextLocation loc = default)
        {
            return new ParameterDeclaration(identifier, option ?? Expression.Null, isVariadic, attribute, loc);
        }

        public static TypeDeclaration MakeClassDecl(string className, IEnumerable<AstType> bases, IEnumerable<EntityDeclaration> decls,
                                                    Modifiers modifiers, IEnumerable<TypeConstraint> constraints = null, AttributeSection attribute = null,
                                                    TextLocation start = default, TextLocation end = default)
        {
            return new TypeDeclaration(ClassType.Class, MakeIdentifier(className), bases, decls, constraints, attribute, modifiers, start, end);
        }

        public static TypeDeclaration MakeClassDecl(Identifier ident, IEnumerable<AstType> bases, IEnumerable<EntityDeclaration> decls,
                                                    Modifiers modifiers, IEnumerable<TypeConstraint> constraints = null, AttributeSection attribute = null,
                                                    TextLocation start = default, TextLocation end = default)
        {
            return new TypeDeclaration(ClassType.Class, ident, bases, decls, constraints, attribute, modifiers, start, end);
        }

        public static TypeDeclaration MakeInterfaceDecl(string interfaceName, IEnumerable<AstType> bases, IEnumerable<EntityDeclaration> decls,
                                                        Modifiers modifiers, IEnumerable<TypeConstraint> constraints = null, AttributeSection attribute = null,
                                                        TextLocation start = default, TextLocation end = default)
        {
            return new TypeDeclaration(ClassType.Interface, MakeIdentifier(interfaceName), bases, decls, constraints, attribute, modifiers, start, end);
        }

        public static TypeDeclaration MakeInterfaceDecl(Identifier ident, IEnumerable<AstType> bases, IEnumerable<EntityDeclaration> decls, Modifiers modifiers,
                                                        IEnumerable<TypeConstraint> constraints =null, AttributeSection attribute = null,
                                                        TextLocation start = default, TextLocation end = default)
        {
            return new TypeDeclaration(ClassType.Interface, ident, bases, decls, constraints, attribute, modifiers, start, end);
        }

        public static FunctionDeclaration MakeFunc(string name, IEnumerable<ParameterDeclaration> parameters, BlockStatement body, AstType returnType,
                                                   Modifiers modifiers, IEnumerable<TypeConstraint> constraints = null, AttributeSection attribute = null,
                                                   TextLocation loc = default)
        {
            return new FunctionDeclaration(MakeIdentifier(name), parameters, constraints, body, returnType, attribute, modifiers, loc);;
        }

        public static FunctionDeclaration MakeFunc(Identifier ident, IEnumerable<ParameterDeclaration> parameters, BlockStatement body, AstType returnType,
                                                   Modifiers modifiers, IEnumerable<TypeConstraint> constraints = null, AttributeSection attribute = null,
                                                   TextLocation loc = default)
        {
            return new FunctionDeclaration(ident, parameters, constraints, body, returnType, attribute, modifiers, loc);
        }

        public static TypeDeclaration MakeEnumDecl(Identifier ident, IEnumerable<EntityDeclaration> decls, Modifiers modifiers, IEnumerable<TypeConstraint> constraints = null,
                                                   AttributeSection attribute = null, TextLocation start = default, TextLocation end = default)
        {
            return new TypeDeclaration(ClassType.Enum, ident, Enumerable.Empty<AstType>(), decls, constraints, attribute, modifiers, start, end);
        }
        #endregion
    }
}

