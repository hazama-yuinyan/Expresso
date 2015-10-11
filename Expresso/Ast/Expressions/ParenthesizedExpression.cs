using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents an expression surrounded by a pair of parenthesis.
    /// Also, a tuple expression is represented as 
    /// ParenthesizedExpression{
    ///     SequenceExpression{
    ///         Expression...
    ///     }
    /// }
    /// '(' SequenceExpression ')' ;
    /// </summary>
    /// <remarks>
    /// Note that this single node doesn't specify any semantics in Expresso.
    /// </remarks>
    public class ParenthesizedExpression : Expression
    {
        public ExpressoTokenNode LPar{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

        /// <summary>
        /// The inner expression.
        /// </summary>
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
            return walker.VisitParenthesizedExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitParenthesizedExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ParenthesizedExpression;
            return o != null && o.Expression.DoMatch(o.Expression, match);
        }

        #endregion
    }
}

