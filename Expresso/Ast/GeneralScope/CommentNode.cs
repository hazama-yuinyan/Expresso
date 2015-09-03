using System;


namespace Expresso.Ast
{
    /// <summary>
    /// コメント。
    /// A Comment node holds the content of comments.
    /// </summary>
    public class CommentNode : AstNode
    {
        /// <summary>
        /// The plain string representing the content.
        /// </summary>
        public string Text{
            get; set;
        }

        public override NodeType NodeType{
            get{
                return NodeType.Whitespace;
            }
        }

        public CommentNode(string content)
        {
            Text = content;
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitCommentNode(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitCommentNode(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitCommentNode(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as CommentNode;
            return o != null && MatchString(Text, o.Text);
        }
        #endregion
    }
}

