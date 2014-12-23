using System;


namespace Expresso.Ast
{
    public enum KeywordExpressions
    {
        Null,
        This
    }

    public class KeywordReferenceExpression : Expression
    {
        public KeywordExpressions ExprType{
            get; set;
        }

        public KeywordReferenceExpression()
        {
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            throw new NotImplementedException();
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

