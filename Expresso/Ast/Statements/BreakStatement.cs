using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// break文。
    /// The break statement.
    /// "break" [ "upto" LiteralExpression ] ';' ;
    /// </summary>
    public class BreakStatement : Statement
    {
        public static readonly TokenRole BreakTokenRole = new TokenRole("break", ExpressoTokenNode.Null);
        public static readonly Role<LiteralExpression> LiteralRole = new Role<LiteralExpression>("Literal");

        /// <summary>
        /// breakの際に何階層分ループ構造を遡るか。
        /// Indicates how many loops we will break out.
        /// </summary>
        public LiteralExpression Count{
            get{return GetChildByRole(LiteralRole);}
            set{SetChildByRole(LiteralRole, value);}
        }

        public BreakStatement(LiteralExpression countExpr, TextLocation start, TextLocation end)
            : base(start, end)
        {
            Count = countExpr;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitBreakStatement(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitBreakStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitBreakStatement(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as BreakStatement;
            return o != null && Count.DoMatch(o.Count, match);
        }
    }
}

