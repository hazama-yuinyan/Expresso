using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the keyword "this" as expression.
    /// </summary>
    public class ThisReferenceExpression : Expression
    {
        public ThisReferenceExpression(TextLocation start)
        {
            start_loc = start;
            end_loc = new TextLocation(start.Line, start.Column + "this".Length);
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
            return other is ThisReferenceExpression;
        }

        #endregion
    }
}

