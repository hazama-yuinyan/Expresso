using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents an Expresso token. Note that the type of the token is defined through the TokenRole.
    /// </summary>
    public class ExpressoTokenNode : AstNode
    {
        #region Null
        public static new readonly ExpressoTokenNode Null = new NullExpressoTokenNode();

        sealed class NullExpressoTokenNode : ExpressoTokenNode
        {
            public override bool IsNull => true;

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

            internal protected override bool DoMatch(AstNode other, Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int TokenLength => TokenRole.TokenLengths[(int)flags >> (int)AstNodeFlagsUsedBits];

        /// <summary>
        /// Gets the token as a string.
        /// </summary>
        public string Token => TokenRole.Tokens[(int)flags >> (int)AstNodeFlagsUsedBits];

        public override NodeType NodeType => NodeType.Token;

        public ExpressoTokenNode(TextLocation location, TokenRole role)
            : base(location, new TextLocation(location.Line,
                location.Column + TokenRole.TokenLengths[(role != null) ? (int)role.TokenIndex : 0]))
        {
            if(role != null)
                flags |= role.TokenIndex << (int)AstNodeFlagsUsedBits;
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

        internal protected override bool DoMatch(AstNode other, Match match)
        {
            ExpressoTokenNode o = other as ExpressoTokenNode;
            return o != null && !o.IsNull && !(o is ExpressoModifierToken);
        }
    }
}

