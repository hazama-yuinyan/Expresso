using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Runtime;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	public class ExprStatement : Statement, CompoundStatement
	{
        /// <summary>
        /// 実行する式のリスト。
        /// </summary>
        public List<Expression> Expressions { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.ExprStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ExprStatement;

            if (x == null) return false;

            return this.Expressions.Equals(x.Expressions);
        }

        public override int GetHashCode()
        {
            return this.Expressions.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			foreach (var expr in Expressions)
				expr.Run(varStore);
			
			return null;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
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

