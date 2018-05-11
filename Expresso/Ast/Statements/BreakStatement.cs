using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// break文。
    /// The break statement.
    /// A break statement brings the flow out of loops certain times.
    /// Note that break statements in Expresso have their effects only on loop statements.
    /// "break" [ "upto" LiteralExpression ] ';' ;
    /// </summary>
    public class BreakStatement : Statement
    {
        public static readonly TokenRole BreakTokenRole = new TokenRole("break", ExpressoTokenNode.Null);
        public static readonly Role<LiteralExpression> LiteralRole =
            new Role<LiteralExpression>("Literal", LiteralExpression.Null);

        /// <summary>
        /// breakの際に何階層分ループ構造を遡るか。
        /// Indicates how many loops we will break out.
        /// This expression has to be evaluated to a positive integer.
        /// Otherwise a compilation error occurs.
        /// </summary>
        public LiteralExpression CountExpression{
            get => GetChildByRole(LiteralRole);
            set => SetChildByRole(LiteralRole, value);
        }

        public BreakStatement(LiteralExpression countExpr, TextLocation start, TextLocation end)
            : base(start, end)
        {
            CountExpression = countExpr;
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

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as BreakStatement;
            return o != null && CountExpression.DoMatch(o.CountExpression, match);
        }
    }
}

