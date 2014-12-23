using System;


namespace Expresso.Ast
{
    /// <summary>
    /// break文。
    /// The break statement.
    /// </summary>
    public class BreakStatement : Statement
    {
        public static readonly TokenRole BreakTokenRole = new TokenRole("break");
        readonly int count;

        /// <summary>
        /// breakの際に何階層分ループ構造を遡るか。
        /// Indicates how many loops we will break out.
        /// </summary>
        public int Count{
            get{return count;}
        }

        /// <summary>
        /// このbreak文が含まれるループ構文。
        /// Loops that have this statement as their child.
        /// </summary>
        /*public IEnumerable<BreakableStatement> Enclosings{
            get{return Children;}
        }*/

        public BreakStatement(int loopCount/*, BreakableStatement[] loops*/)
        {
            count = loopCount;

            //foreach(var enclosing in loops)
            //    AddChild(enclosing);
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

        protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as BreakStatement;
            return o != null && Count == o.Count;
        }
    }
}

