using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the keyword "super" as expression.
    /// "super" ;
    /// </summary>
    public class SuperReferenceExpression : Expression
    {
        public SuperReferenceExpression(TextLocation loc)
            : base(loc, new TextLocation(loc.Line, loc.Column + "super".Length))
        {
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitSuperReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSuperReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitSuperReferenceExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            return other is SuperReferenceExpression;
        }

        #endregion
    }
}

