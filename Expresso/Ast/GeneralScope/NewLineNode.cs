using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a line break in the text.
    /// </summary>
    public sealed class NewLineNode : AstNode
    {
        public override NodeType NodeType{
            get{
                return NodeType.Whitespace;
            }
        }

        const uint NewLineMask = 0xfu << AstNodeFlagsUsedBits;
        static readonly UnicodeNewline[] NewLineTypes = {
            UnicodeNewline.Unknown,
            UnicodeNewline.LF,
            UnicodeNewline.CRLF,
            UnicodeNewline.CR,
            UnicodeNewline.NEL,
            UnicodeNewline.VT,
            UnicodeNewline.FF,
            UnicodeNewline.LS,
            UnicodeNewline.PS
        };

        public UnicodeNewline NewLineType{
            get{
                return NewLineTypes[(flags & NewLineMask) >> AstNodeFlagsUsedBits];
            }

            set{
                ThrowIfFrozen();
                int pos = Array.IndexOf(NewLineTypes, value);
                if(pos < 0)
                    pos = 0;

                flags &= ~NewLineMask;  //clear old newline type
                flags |= (uint)pos << AstNodeFlagsUsedBits;
            }
        }

        public NewLineNode(TextLocation location)
        {
            start_loc = location;
            end_loc = new TextLocation(location.Line + 1, 1);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitNewLine(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitNewLine(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitNewLine(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            return other is NewLineNode;
        }
    }
}

