using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents the rest pattern.
    /// A rest pattern matches the remaining elements or fields.
    /// </summary>
    public class IgnoringRestPattern : PatternConstruct
    {
        public IgnoringRestPattern(TextLocation loc)
            : base(loc, new TextLocation(loc.Line, "..".Length + loc.Column))
        {
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitIgnoringRestPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitIgnoringRestPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitIgnoringRestPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            return other == null || other is IgnoringRestPattern;
        }
    }
}
