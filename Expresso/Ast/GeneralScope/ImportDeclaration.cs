using System.Collections.Generic;
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
    /// Above all, the name should represent a full path to a module, a type, a function or a module field.
    /// "import" ident [ "::" ( ident | '{' ident { ',' ident } '}' ) ] { '.' ( ident | '{' ident { ',' ident } '}' ) [ "from" string_literal ] "as" ( ident | '{' ident { ',' ident } '}') ';'
    /// </summary>
    public class ImportDeclaration : AstNode
    {
        public static readonly TokenRole ImportKeyword =
            new TokenRole("import", ExpressoTokenNode.Null);
        public static readonly Role<Identifier> ImportPathRole =
            new Role<Identifier>("ImportPath", Identifier.Null);
        public static readonly Role<Identifier> TargetFileRole =
            new Role<Identifier>("TargetFile", Identifier.Null);
        public static readonly Role<Identifier> AliasNameRole =
            new Role<Identifier>("AliasName", Identifier.Null);

        public ExpressoTokenNode ImportToken{
            get{return GetChildByRole(ImportKeyword);}
        }

        /// <summary>
        /// インポート対象となるモジュールパス。
        /// The target module path to import names from or the target namespace name to import names from.
        /// The file path has to contain the module name that it refers to.
        /// </summary>
        /*public string ModuleName{
            get{return ImportPaths.Name;}
		}*/

        /// <summary>
        /// Gets the import paths.
        /// An import path represents the target name that we'll be importing.
        /// An import path can refer to a module, a type, a function or a module variable.
        /// </summary>
        /// <value>The import paths.</value>
        public AstNodeCollection<Identifier> ImportPaths{
            get{return GetChildrenByRole(ImportPathRole);}
        }

        public string TargetFilePath{
            get{return TargetFile.Name;}
        }

        /// <summary>
        /// Represents the target file.
        /// It can be a null node if this import refers to a type in the standard library.
        /// </summary>
        /// <value>The target file.</value>
        public Identifier TargetFile{
            get{return GetChildByRole(TargetFileRole);}
            set{SetChildByRole(TargetFileRole, value);}
        }

		/// <summary>
		/// モジュールに対して与えるエイリアス名。
        /// An alias name that can be used to refer to the module within the scope.
        /// It can be empty if none is specified.
		/// </summary>
        /*public string AliasName{
            get{return AliasTokens.Name;}
		}*/

        public ExpressoTokenNode AsToken{
            get{return GetChildByRole(Roles.AsToken);}
        }

        /// <summary>
        /// Gets alias names only valid within the current scope.
        /// </summary>
        public AstNodeCollection<Identifier> AliasTokens{
            get{return GetChildrenByRole(AliasNameRole);}
        }

        public ExpressoTokenNode SemicolonToken{
            get{return GetChildByRole(Roles.SemicolonToken);}
        }

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

        public ImportDeclaration(IEnumerable<Identifier> paths, IEnumerable<Identifier> aliases, Identifier targetFile, TextLocation start, TextLocation end)
            : base(start, end)
        {
            ImportPaths.AddRange(paths);
            AliasTokens.AddRange(aliases);
            TargetFile = targetFile;
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
            return o != null && ImportPaths.DoMatch(o.ImportPaths, match)
                                           && AliasTokens.DoMatch(o.AliasTokens, match) && TargetFile.DoMatch(o.TargetFile, match);
        }

        #endregion
    }
}
