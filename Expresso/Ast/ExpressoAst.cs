using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
#if NETCOREAPP2_0
using System;
using Expresso.TypeSystem;
#endif


namespace Expresso.Ast
{
	/// <summary>
	/// ExpressoのASTのトップレベルノード。モジュールを表す。
	/// Top-level ast for all Expresso code. Represents a module.
    /// All module-level items can be considered as static because one module can import
    /// another module only once.
	/// </summary>
    public class ExpressoAst : AstNode
	{
        public static readonly Role<ExpressoAst> ModuleRole = new Role<ExpressoAst>("Module");
        public static readonly Role<EntityDeclaration> MemberRole = new Role<EntityDeclaration>("Member");
        public static readonly Role<ImportDeclaration> ImportRole = new Role<ImportDeclaration>("Import");

        /// <summary>
        /// Represents the name token.
        /// </summary>
        /// <value>The name token.</value>
        public Identifier NameToken{
            get => GetChildByRole(Roles.Identifier);
            set => SetChildByRole(Roles.Identifier, value);
        }

		/// <summary>
		/// モジュール名。
		/// The name of the module.
		/// If the module contains the main function, it should be called the "main" module.
		/// </summary>
        public string Name => NameToken.Name;

        /// <summary>
        /// Gets the external modules.
        /// It can be an empty collection if there is no external modules.
        /// </summary>
        public AstNodeCollection<ExpressoAst> ExternalModules => GetChildrenByRole(ModuleRole);

        /// <summary>
        /// インポート宣言。
        /// Gets the import declarations.
        /// It can be an empty collection if there is no import declarations.
        /// </summary>
        public AstNodeCollection<ImportDeclaration> Imports => GetChildrenByRole(ImportRole);

		/// <summary>
		/// 本体。
		/// The body of this node.
		/// </summary>
        public AstNodeCollection<EntityDeclaration> Declarations => GetChildrenByRole(MemberRole);

        /// <summary>
        /// Represents the attribute associated with the module.
        /// </summary>
        /// <value>The attribute.</value>
        public AstNodeCollection<AttributeSection> Attributes => GetChildrenByRole(EntityDeclaration.AttributeRole);

        /// <summary>
        /// モジュール名(デバッグ用)
        /// Gets the name of the module as pretty string.(especially for debugging)
        /// </summary>
        /// <value>The name of the module.</value>
        public string ModuleName => $"<module {Name}>";

        public override NodeType NodeType => NodeType.TypeDeclaration;

        public ExpressoAst(IEnumerable<EntityDeclaration> decls, IEnumerable<ImportDeclaration> imports, string maybeModuleName, IEnumerable<AttributeSection> attributes)
            : base(new TextLocation(1, 1), decls.Last().EndLocation)
        {
            NameToken = MakeIdentifier(maybeModuleName);

            if(imports != null)
                Imports.AddRange(imports);

            Declarations.AddRange(decls);
            if(attributes != null)
                Attributes.AddRange(attributes);
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitAst(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitAst(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitAst(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ExpressoAst;
            return o != null && Declarations.DoMatch(o.Declarations, match) && NameToken.DoMatch(o.NameToken, match)
                                            && Attributes.DoMatch(o.Attributes, match);
        }

        #endregion

        #if NETCOREAPP2_0
        public ExpressoUnresolvedFile ToTypeSystem()
        {
            if(string.IsNullOrEmpty(Name))
                throw new InvalidOperationException("Can not use ToTypeSystem() on a syntax tree without file name.");

            var walker = new TypeSystemConvertWalker(Name);
            AcceptWalker(walker);
            return walker.UnresolvedFile;
        }
        #endif
	}
}

