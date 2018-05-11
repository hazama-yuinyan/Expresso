using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// 空の文。
    /// Represents an empty statement.
    /// ';' ;
    /// </summary>
    public class EmptyStatement : Statement
    {
        public EmptyStatement(TextLocation location)
            : base(location, new TextLocation(location.Line, location.Column + ";".Length))
		{
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitEmptyStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitEmptyStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitEmptyStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            return other is EmptyStatement;
        }

        #endregion
    }
}
