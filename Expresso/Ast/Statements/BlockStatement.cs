﻿using System.Collections.Generic;
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
        /// <summary>
        /// ブロックの中身の文。
		/// The body statements
        /// </summary>
        public AstNodeCollection<Statement> Statements{
            get{return GetChildrenByRole(Roles.EmbeddedStatement);}
		}

        public BlockStatement(IEnumerable<Statement> stmts, TextLocation start, TextLocation end)
            : base(start, end)
        {
            foreach(var stmt in stmts)
                AddChild(stmt, Roles.EmbeddedStatement);
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