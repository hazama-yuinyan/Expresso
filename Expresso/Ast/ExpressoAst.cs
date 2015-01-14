using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Expresso.Compiler;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/// <summary>
	/// ExpressoのASTのトップレベルノード。ファイルから生成された場合はモジュールを表すが、evalによって作られた場合もある。
	/// Top-level ast for all Expresso code. Typically represents a module but could also
	/// be exec or eval code.
	/// </summary>
    public class ExpressoAst : AstNode
	{
        public static readonly Role<EntityDeclaration> MemberRole = new Role<EntityDeclaration>("Member", EntityDeclaration.Null);
        public static readonly Role<ImportDeclaration> ImportRole = new Role<ImportDeclaration>("Import", ImportDeclaration.Null);

        readonly string name;
		readonly bool is_module;

		/// <summary>
		/// モジュール名。
		/// The name of the module.
		/// If the module contains the main function, it would be called the "main" module.
		/// </summary>
		public string Name{
            get{return is_module ? string.Format("<module {0}>", name) : ModuleName;}
		}

		/// <summary>
		/// Indicates whether the node represents a module.
		/// </summary>
		public bool IsModule{
			get{return is_module;}
		}

        /// <summary>
        /// インポート宣言
        /// Gets the import declarations.
        /// If this node doesn't represent a module, it will be an empty collection.
        /// </summary>
        /// <value>The imports.</value>
        public AstNodeCollection<ImportDeclaration> Imports{
            get{return GetChildrenByRole(ImportRole);}
        }

		/// <summary>
		/// 本体。このノードがモジュールだった場合にはモジュールの定義文が含まれる。
		/// The body of this node. If this node represents a module, then the body includes
		/// the definitions of the module.
		/// </summary>
        public AstNodeCollection<EntityDeclaration> Body{
            get{return GetChildrenByRole(Roles.Body);}
		}

        /// <summary>
        /// モジュール名
        /// Gets the name of the module.
        /// </summary>
        /// <value>The name of the module.</value>
		public string ModuleName{
			get{
				return is_module ? name : "<not a module>";
			}
		}

		public override NodeType NodeType{
            get{return NodeType.Unknown;}
        }

        public ExpressoAst(IEnumerable<EntityDeclaration> body, IEnumerable<ImportDeclaration> imports, string maybeModuleName = null)
        {
			name = maybeModuleName;
			is_module = maybeModuleName != null;

            if(imports != null){
                foreach(var import in imports)
                    AddChild(import, ImportRole);
            }

            foreach(var decl in body)
                AddChild(decl, MemberRole);
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
            return o != null && Body.DoMatch(o.Body, match) && Name == o.Name;
        }

        #endregion

        public ExpressoUnresolvedFile ToTypeSystem()
        {
            if(string.IsNullOrEmpty(name))
                throw new InvalidOperationException("Can not use ToTypeSystem() on a syntax tree without file name.");

            var type_def = new DefaultUnresolvedTypeDefinition("global");
            var walker = new TypeSystemConvertWalker(new ExpressoUnresolvedFile(name, type_def, null));
            walker.AcceptWalker(this);
            return walker.UnresolvedFile;
        }
	}
}

