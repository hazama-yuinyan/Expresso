using System;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// メソッド定義。
    /// The method declaration.
    /// </summary>
    public class MethodDeclaration : EntityDeclaration
    {
        public override ICSharpCode.NRefactory.TypeSystem.SymbolKind SymbolKind{
            get{
                return SymbolKind.Method;
            }
        }

        public ExpressoTokenNode LParenthesis{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

        public AstNodeCollection<ParameterDeclaration> Parameters{
            get{return GetChildrenByRole(Roles.Parameter);}
        }

        public ExpressoTokenNode RParenthesis{
            get{return GetChildByRole(Roles.RParenthesisToken);}
        }

        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
        }

        public MethodDeclaration(IEnumerable<ParameterDeclaration> parameters, BlockStatement body)
        {
            foreach(var item in parameters)
                AddChild(item, Roles.Parameter);

            SetChildByRole(body, Roles.Body);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitMethodDeclaration(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitMethodDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitMethodDeclaration(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as MethodDeclaration;
            return o != null && MatchString(Name, o.Name) && ReturnType.DoMatch(o.ReturnType, match)
                && Parameters.DoMatch(o.Parameters, match) && Body.DoMatch(o.Body, match);
        }
    }
}

