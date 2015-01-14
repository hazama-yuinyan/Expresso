using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the keyword "self" as expression.
    /// </summary>
    public class SelfReferenceExpression : Expression
    {
        public SelfReferenceExpression(TextLocation start)
        {
            start_loc = start;
            end_loc = new TextLocation(start.Line, start.Column + "self".Length);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitSelfReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSelfReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitSelfReferenceExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            return other is SelfReferenceExpression;
        }

        #endregion
    }
}

