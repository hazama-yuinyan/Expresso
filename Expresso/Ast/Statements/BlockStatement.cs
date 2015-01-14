using System.Collections.Generic;
using System.Linq;
using Expresso.Runtime;
using Expresso.Compiler;

namespace Expresso.Ast
{
    /// <summary>
    /// 複文ブロック。
	/// Represents a block of statements.
    /// '{' { Statement } '}'
    /// </summary>
	/// <seealso cref="BreakableStatement"/>
    public class BlockStatement : Statement
    {
        /// <summary>
        /// ブロックの中身の文。
		/// The body statements
        /// </summary>
        public AstNodeCollection<Statement> Statements{
            get{GetChildrenByRole(Roles.EmbeddedStatement);}
		}

        public BlockStatement(IEnumerable<Statement> stmts)
        {
            foreach(var stmt in stmts)
                AddChild(Roles.EmbeddedStatement, stmt);
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
