using System.Collections.Generic;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
    /// <summary>
    /// 複文ブロック。
	/// Represents a block of statements.
    /// '{' { Statement } '}' ;
    /// </summary>
    public class BlockStatement : Statement
    {
        #region Null
        public static readonly new BlockStatement Null = new NullBlockStatement();

        sealed class NullBlockStatement : BlockStatement
        {
            public override bool IsNull => true;

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
        #endregion

        /// <summary>
        /// ブロックの中身の文。
		/// The body statements
        /// </summary>
        public AstNodeCollection<Statement> Statements => GetChildrenByRole(Roles.EmbeddedStatement);

        protected BlockStatement()
        {
        }

        public BlockStatement(IEnumerable<Statement> stmts, TextLocation start, TextLocation end)
            : base(start, end)
        {
            Statements.AddRange(stmts);
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitBlock(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitBlock(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitBlock(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as BlockStatement;
            return o != null && Statements.DoMatch(o.Statements, match);
        }
    }
}
