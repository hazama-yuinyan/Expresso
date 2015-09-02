using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents an expression pattern.
    /// An expression pattern matches an expression.
    /// Expression ;
    /// </summary>
    public class ExpressionPattern : PatternConstruct
    {
        /// <summary>
        /// Represents the inner expression.
        /// </summary>
        public Expression Expression{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        public ExpressionPattern(Expression expr)
        {
            Expression = expr;
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitExpressionPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitExpressionPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitExpressionPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ExpressionPattern;
            return o != null && Expression.DoMatch(o.Expression, match);
        }

        #endregion
    }
}

