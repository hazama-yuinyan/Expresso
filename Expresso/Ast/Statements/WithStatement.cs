using System;
using System.Collections.Generic;

using Expresso.Runtime;
using Expresso.Compiler;

namespace Expresso.Ast
{
	/// <summary>
	/// With文.
	/// The With statement.
	/// </summary>
	/// <seealso cref="Node"/>
	public class WithStatement : Statement
	{
		readonly Expression context;
		readonly Statement body;

		/// <summary>
        /// 自動破棄の対象となるリソースを返す式。
		/// The expression that returns a resource.
        /// </summary>
        public Expression ContextExpr{
			get{return context;}
		}

        /// <summary>
        /// Main式で評価したリソースを使用する式。
		/// A statement or a block that uses the resource acquired in the "Main" expression.
        /// </summary>
        public Statement Body{
			get{return body;}
		}

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

		public WithStatement(Expression contextExpr, Statement bodyStmt)
		{
			context = contextExpr;
			body = bodyStmt;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitWithStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitWithStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitWithStatement(this, data);
        }

		public IEnumerable<Identifier> CollectLocalVars()
		{
			return null;
		}
	}
}

