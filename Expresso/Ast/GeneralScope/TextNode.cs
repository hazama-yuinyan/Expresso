using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// A Text node contains arbitrary strings.
    /// In other words, it represents a non-parsible part of source codes.
    /// </summary>
    public class TextNode : AstNode
    {
        public string Text{
            get; set;
        }

        public override NodeType NodeType => NodeType.Whitespace;

        public TextNode(string text) : this(text, TextLocation.Empty, TextLocation.Empty)
        {
        }

        public TextNode(string text, TextLocation startLoc, TextLocation endLoc)
            : base(startLoc, endLoc)
        {
            Text = text;
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitTextNode(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitTextNode(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitTextNode(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as TextNode;
            return o != null && Text == o.Text;
        }

        #endregion
    }
}

