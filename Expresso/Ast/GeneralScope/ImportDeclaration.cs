using System;
using System.Collections.Generic;
using System.Linq;

using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
    /// <summary>
    /// モジュールインポート宣言。
    /// Reperesents an import declaration.
    /// A module import can be done in 2 phases.
    /// 1. Path resolving.
    /// 2. Name imports.
    /// Path resolving can be done as follows:
    /// Above all, the string literal should represent a full path to a source file or on .NET environment
    /// a full path to a type provided by the standard library.
    /// "import" string_literal [ "as" ident ] ';'
    /// </summary>
    public class ImportDeclaration : AstNode
    {
        public static readonly TokenRole ImportKeyword =
            new TokenRole("import", ExpressoTokenNode.Null);
        public static readonly Role<Identifier> ModuleNameRole =
            new Role<Identifier>("ModuleName", Identifier.Null);
        public static readonly Role<Identifier> AliasNameRole =
            new Role<Identifier>("AliasName", Identifier.Null);
        //public static readonly Role<PathExpression> ImportedEntityRole =
        //    new Role<PathExpression>("ImportedEntity", PathExpression.Null);

        public ExpressoTokenNode ImportToken{
            get{return GetChildByRole(ImportKeyword);}
        }

        /// <summary>
        /// インポート対象となるモジュールパス。
        /// The target module path to import names from or the target namespace name to import names from.
        /// The file path has to contain the module name that it refers to.
        /// </summary>
        public string ModuleName{
            get{return ModuleNameToken.Name;}
		}

        public Identifier ModuleNameToken{
            get{return GetChildByRole(ModuleNameRole);}
            set{SetChildByRole(ModuleNameRole, value);}
        }

		/// <summary>
		/// モジュールに対して与えるエイリアス名。
        /// An alias name that can be used to refer to the module within the scope.
        /// It can be empty if none is specified.
		/// </summary>
        public string AliasName{
            get{return AliasNameToken.Name;}
		}

        public ExpressoTokenNode AsToken{
            get{return GetChildByRole(Roles.AsToken);}
        }

        /// <summary>
        /// Gets alias name only valid within the current scope.
        /// </summary>
        public Identifier AliasNameToken{
            get{return GetChildByRole(AliasNameRole);}
            set{SetChildByRole(AliasNameRole, value);}
        }

        public ExpressoTokenNode InToken{
            get{return GetChildByRole(Roles.InToken);}
        }

        /// <summary>
        /// Gets imported entities.
        /// An imported entity can be any static item.
        /// It can return an empty collection if the node represents the module-names-aliases pair.
        /// </summary>
        /*public AstNodeCollection<PathExpression> ImportedEntities{
            get{return GetChildrenByRole(ImportedEntityRole);}
        }*/

        public ExpressoTokenNode SemicolonToken{
            get{return GetChildByRole(Roles.SemicolonToken);}
        }

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

        public ImportDeclaration(Identifier moduleName, Identifier alias = null,
            IEnumerable<PathExpression> importedEntities = null)
        {
            ModuleNameToken = moduleName;

            if(alias != null)
                AliasNameToken = alias;

            //if(importedEntities != null)
            //    ImportedEntities.AddRange(importedEntities);
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
            return o != null && ModuleNameToken.DoMatch(o.ModuleNameToken, match)
                && AliasNameToken.DoMatch(o.AliasNameToken, match);
                //&& ImportedEntities.DoMatch(o.ImportedEntities, match);
        }

        #endregion
    }
}
