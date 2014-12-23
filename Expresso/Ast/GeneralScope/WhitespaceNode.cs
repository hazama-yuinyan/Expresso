using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// A whitespace node contains only whitespaces.
    /// </summary>
    public class WhitespaceNode : AstNode
    {
        public override NodeType NodeType{
            get{
                return NodeType.Whitespace;
            }
        }

        public string WhitespaceText{
            get; private set;
        }



        public WhitespaceNode(string whitespaceText, TextLocation location)
        {
            WhitespaceText = whitespaceText;
            start_loc = location;
            end_loc = new TextLocation(location.Line, location.Column + whitespaceText.Length);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitWhitespace(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitWhitespace(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitWhitespace(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as WhitespaceNode;
            return o != null && WhitespaceText == o.WhitespaceText;
        }
    }
}

