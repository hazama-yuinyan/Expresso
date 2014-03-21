using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Runtime;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// 式文。
	/// The expression statement.
	/// </summary>
	public class ExprStatement : Statement, CompoundStatement
	{
		readonly Expression[] exprs;

        /// <summary>
        /// 実行する式のリスト。
		/// The list of expressions to be evaluated.
        /// </summary>
        public Expression[] Expressions{
			get{return exprs;}
		}

        public override NodeType Type{
            get{return NodeType.ExprStatement;}
        }

		public ExprStatement(Expression[] expressions)
		{
			exprs = expressions;
		}

        public override bool Equals(object obj)
        {
            var x = obj as ExprStatement;

            if(x == null)
                return false;

            return exprs.Equals(x.exprs);
        }

        public override int GetHashCode()
        {
            return this.exprs.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			foreach (var expr in Expressions)
				expr.Run(varStore);
			
			return null;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(AstWalker walker)
		{
			if(walker.Walk(this)){
				foreach(var e in exprs)
					e.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			var result = 
				from p in Expressions
				where p.Type == NodeType.VarDecl
				select ImplementationHelpers.CollectLocalVars(p) into t
				from q in t
				select q;

			return result;
		}
	}
}

