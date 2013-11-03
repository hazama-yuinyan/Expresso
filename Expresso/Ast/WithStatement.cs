using System;
using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Builtins.Library;
using Expresso.Runtime;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// With文.
	/// The With statement.
	/// </summary>
	/// <seealso cref="Node"/>
	public class WithStatement : Statement, CompoundStatement
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

        public override NodeType Type
        {
            get { return NodeType.WithStatement; }
        }

		public WithStatement(Expression contextExpr, Statement bodyStmt)
		{
			context = contextExpr;
			body = bodyStmt;
		}

        public override bool Equals(object obj)
        {
            var x = obj as WithStatement;

            if (x == null) return false;

            return context == x.context && body.Equals(x.body);
        }

        public override int GetHashCode()
        {
            return context.GetHashCode() ^ body.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			var resource = Main.Run(varStore) as IClosable;
			if(resource == null)
				throw ExpressoOps.InvalidTypeError("Can not evaluate the expression to a valid closable object.");

			try{
				Body.Run(varStore);
			}
			finally{
				resource.Close();
			}

			return null;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				context.Walk(walker);
				body.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			return null;
		}
	}
}

