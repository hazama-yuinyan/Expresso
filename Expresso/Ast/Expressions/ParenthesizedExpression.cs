using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a parenthesized expression.
    /// '(' Expression ')'
    /// </summary>
    public class ParenthesizedExpression : Expression
    {
        public ExpressoTokenNode LPar{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

        public Expression Expression{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        public ExpressoTokenNode RPar{
            get{return GetChildByRole(Roles.RParenthesisToken);}
        }

        public ParenthesizedExpression(Expression expression)
        {
            Expression = expression;
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitParenthesizedExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            throw new NotImplementedException();
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            throw new NotImplementedException();
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

