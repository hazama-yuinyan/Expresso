using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// continue文。
    /// The continue statement.
    /// A continue statement brings the flow out of loops certain times and then continue execution
    /// on that loop.
    /// "continue" [ "upto" Expression ] ';' ;
    /// </summary>
    public class ContinueStatement : Statement
    {
        public static readonly TokenRole ContinueTokenRole = new TokenRole("continue", ExpressoTokenNode.Null);

        /// <summary>
        /// continueの際に何階層分ループ構造を遡るか。
        /// Indicates how many loops we will break out.
        /// </summary>
        public LiteralExpression CountExpression{
            get{return GetChildByRole(BreakStatement.LiteralRole);}
            set{SetChildByRole(BreakStatement.LiteralRole, value);}
        }

        public ContinueStatement(LiteralExpression countExpr, TextLocation start, TextLocation end)
            : base(start, end)
        {
            CountExpression = countExpr;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitContinueStatement(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitContinueStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitContinueStatement(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ContinueStatement;
            return o != null && CountExpression.DoMatch(o.CountExpression, match);
        }
    }
}

