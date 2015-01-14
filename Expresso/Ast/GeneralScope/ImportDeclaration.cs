using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Compiler;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
    /// <summary>
    /// モジュールインポート宣言。
    /// Reperesents an import declaration.
    /// A module import can be done in 2 phases.
    /// 1. Path resolving.
    /// 2. Name imports.
    /// "import" ModuleName [ "as" AliasName ] {',' ModuleName [ "as" AliasName ] }
    /// | 
    /// </summary>
    public class ImportDeclaration : AstNode
    {
        public static readonly TokenRole ImportKeyword = new TokenRole("import");
        public static readonly TokenRole AsKeyword = new TokenRole("as");
        public static readonly Role<Identifier> ModuleNameRole = new Role<Identifier>("ModuleName", Identifier.Null);
        public static readonly Role<Identifier> AliasNameRole = new Role<Identifier>("AliasName", Identifier.Null);
        public static readonly Role<Identifier> ImportedEntityRole = new Role<Identifier>("ImportedEntity", Identifier.Null);

        public ExpressoTokenNode ImportToken{
            get{return GetChildByRole(ImportKeyword);}
        }

        /// <summary>
        /// 対象となるモジュール名。
        /// The target module names to be imported. It can contain a '.'(which can be used to point to a nested module)
        /// or a '/'(which can be used to point to an external source file).
        /// </summary>
        public IEnumerable<string> ModuleNames{
            get{
                var names =
                    from ident in GetChildrenByRole(ModuleNameRole)
                    select ident.Name;
                return names;
            }
		}

        public AstNodeCollection<Identifier> ModuleNameTokens{
            get{return GetChildrenByRole(ModuleNameRole);}
        }

		/// <summary>
		/// モジュールに対して与えるエイリアス名。
        /// Alias names that can be used to refer to the modules within the scope.
		/// It can be null if none is specified.
		/// </summary>
        public IEnumerable<string> AliasNames{
			get{
                var names =
                    from ident in GetChildrenByRole(AliasNameRole)
                    select ident.Name;
                return names;
            }
		}

        public ExpressoTokenNode AsToken{
            get{return GetChildByRole(AsToken);}
        }

        public AstNodeCollection<Identifier> AliasNameTokens{
            get{return GetChildrenByRole(AliasNameRole);}
        }

        public ExpressoTokenNode InToken{
            get{return GetChildByRole(InToken);}
        }

        public AstNodeCollection<Identifier> ImportedEntities{
            get{return GetChildrenByRole(ImportedEntityRole);}
        }

        public ExpressoTokenNode SemicolonToken{
            get{return GetChildByRole(Roles.SemicolonToken);}
        }

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

        public ImportDeclaration(IEnumerable<string> moduleNames, IEnumerable<string> aliasNames = null)
		{
            foreach(var module_name in moduleNames)
                AddChild(AstNode.MakeIdentifier(module_name), ModuleNameRole);

            if(aliasNames != null){
                foreach(var alias_name in aliasNames)
                    AddChild(AstNode.MakeIdentifier(alias_name), AliasNameRole);
            }
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitImportDeclaration(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitImportDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitImportDeclaration(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ImportDeclaration;
            return o != null && ModuleNameTokens.DoMatch(o.ModuleNameTokens, match)
                && AliasNameTokens.DoMatch(o.AliasNameTokens, match);
        }

        #endregion
    }
}
