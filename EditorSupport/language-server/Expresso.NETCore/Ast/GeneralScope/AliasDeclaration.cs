using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents an alias declaration.
    /// An alias declaration, as the name implies, introduces a new name that corresponds to
    /// an existing type, a module-level variable or a module itself.
    /// "alias" PathExpression Identifier ';' ;
    /// </summary>
    public class AliasDeclaration : AstNode
    {
        /// <summary>
        /// The target path to be aliased.
        /// </summary>
        /// <value>The path.</value>
        public PathExpression Path{
            get{return GetChildByRole(Roles.Path);}
            set{SetChildByRole(Roles.Path, value);}
        }

        /// <summary>
        /// The new name introduced by the declaration.
        /// </summary>
        /// <value>The name of the alias.</value>
        public string AliasName{
            get{return AliasToken.Name;}
        }

        /// <summary>
        /// Gets or sets the new name as an identifier.
        /// </summary>
        public Identifier AliasToken{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public AliasDeclaration(PathExpression path, string alias)
        {
            Path = path;
            AliasToken = AstNode.MakeIdentifier(alias);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitAliasDeclaration(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitAliasDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitAliasDeclaration(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as AliasDeclaration;
            return o != null && Path.DoMatch(o.Path, match) && MatchString(AliasName, o.AliasName);
        }

        public override NodeType NodeType{
            get{
                return NodeType.TypeDeclaration;
            }
        }

        #endregion
    }
}

