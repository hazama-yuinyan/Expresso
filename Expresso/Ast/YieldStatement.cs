using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// yield文。
	/// The yield statement.
    /// </summary>
    public class YieldStatement : Statement
    {
		readonly Expression expr;

        /// <summary>
        /// yieldする式。
        /// </summary>
        public Expression Expression{
			get{return expr;}
		}

        public override NodeType Type{
            get{return NodeType.YieldStatement;}
        }

		public YieldStatement(Expression expression)
		{
			expr = expression;
		}

        public override bool Equals(object obj)
        {
            var x = obj as YieldStatement;

            if(x == null)
                return false;

            return expr.Equals(x.expr);
        }

        public override int GetHashCode()
        {
            return expr.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			return null;
        }*/

		internal override void Walk(AstWalker walker)
		{
			if(walker.Walk(this))
				expr.Walk(walker);

			walker.PostWalk(this);
		}

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}
    }
}
