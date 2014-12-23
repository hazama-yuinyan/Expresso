using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents an Expresso token. Note that the type of the token is defined through the TokenRole.
    /// </summary>
    public class ExpressoTokenNode : AstNode
    {
        public static new readonly ExpressoTokenNode Null = new NullExpressoTokenNode();
        sealed class NullExpressoTokenNode : ExpressoTokenNode
        {
            public override bool IsNull{
                get{
                    return true;
                }
            }

            public NullExpressoTokenNode() : base(TextLocation.Empty, null)
            {
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitNullNode(this, data);
            }

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }

        public override NodeType NodeType{
            get{
                return NodeType.Token;
            }
        }

        public ExpressoTokenNode(TextLocation location, TokenRole role)
        {
            start_loc = location;
            if(role != null)
                flags |= role.TokenIndex << AstNodeFlagsUsedBits;

            int token_length = TokenRole.TokenLengths[(int)(flags >> AstNodeFlagsUsedBits)];
            end_loc = new TextLocation(location.Line, location.Column + token_length);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitExpressoTokenNode(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitExpressoTokenNode(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitExpressoTokenNode(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            ExpressoTokenNode o = other as ExpressoTokenNode;
            return o != null && !o.IsNull && !(o is ExpressoModifierToken);
        }
    }
}

