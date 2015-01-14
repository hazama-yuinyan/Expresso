using System;


namespace Expresso.Ast
{
    /// <summary>
    /// continue文。
    /// The continue statement.
    /// "continue" [ "upto" Expression ] ';'
    /// </summary>
    public class ContinueStatement : Statement
    {
        public static readonly TokenRole ContinueTokenRole = new TokenRole("continue");

        /// <summary>
        /// continueの際に何階層分ループ構造を遡るか。
        /// Indicates how many loops we will break out.
        /// </summary>
        public Expression Count{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        /// <summary>
        /// continue文が含まれるループ構文。
        /// Loops that have this statement as their child.
        /// </summary>
        /*public IEnumerable<BreakableStatement> Enclosings{
            get{return Children;}
        }*/

        public ContinueStatement(Expression countExpr/*, BreakableStatement[] loops*/)
        {
            Count = countExpr;

            //foreach(var enclosing in loops)
            //    AddChild(enclosing);
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

        protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ContinueStatement;
            return o != null && Count.DoMatch(o.Count, match);
        }
    }
}

