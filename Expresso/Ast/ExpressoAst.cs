using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP2_0
using System;
using Expresso.TypeSystem;
#endif
using ICSharpCode.NRefactory;

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

        public Identifier NameToken{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

		/// <summary>
		/// モジュール名。
		/// The name of the module.
		/// If the module contains the main function, it should be called the "main" module.
		/// </summary>
		public string Name{
            get{return NameToken.Name;}
		}

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
		/// 本体。このノードがモジュールだった場合にはモジュールの定義文が含まれる。
		/// The body of this node. If this node represents a module, then the body includes
		/// the definitions of the module.
		/// </summary>
        public AstNodeCollection<EntityDeclaration> Declarations => GetChildrenByRole(MemberRole);

        /// <summary>
        /// モジュール名(デバッグ用)
        /// Gets the name of the module as pretty string.
        /// </summary>
        /// <value>The name of the module.</value>
		public string ModuleName{
			get{
                return string.Format("<module {0}>", Name);
			}
		}

		public override NodeType NodeType{
            get{return NodeType.TypeDeclaration;}
        }

        public ExpressoAst(IEnumerable<EntityDeclaration> decls, IEnumerable<ImportDeclaration> imports, string maybeModuleName)
            : base(new TextLocation(1, 1), decls.Last().EndLocation)
        {
            NameToken = MakeIdentifier(maybeModuleName);

            if(imports != null)
                Imports.AddRange(imports);

            Declarations.AddRange(decls);
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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ExpressoAst;
            return o != null && Declarations.DoMatch(o.Declarations, match) && NameToken.DoMatch(o.NameToken, match);
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

