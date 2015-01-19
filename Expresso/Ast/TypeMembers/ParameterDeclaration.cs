using System;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// 仮引数定義。
    /// Represents a parameter(which appears in function signatures).
    /// </summary>
    public class ParameterDeclaration : EntityDeclaration
    {
        public ExpressoTokenNode AssignToken{
            get{return GetChildByRole(Roles.AssignToken);}
        }

        /// <summary>
        /// この引数のデフォルト値。
        /// The optional value for this argument. It would be null if none is specified.
        /// </summary>
        public Expression Option{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        #region implemented abstract members of EntityDeclaration

        public override SymbolKind SymbolKind{
            get{
                return SymbolKind.Parameter;
            }
        }

        #endregion

        public ParameterDeclaration(string name, AstType type, Expression option = null)
        {
            AddChild(AstNode.MakeIdentifier(name), Roles.Identifier);
            AddChild(type, Roles.Type);
            Option = option ?? Expression.Null;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitParameterDeclaration(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitParameterDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitParameterDeclaration(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ParameterDeclaration;
            return o != null && ReturnType.DoMatch(o.ReturnType, match) && MatchString(Name, o.Name)
                && Option.DoMatch(o.Option, match);
        }
    }
}

