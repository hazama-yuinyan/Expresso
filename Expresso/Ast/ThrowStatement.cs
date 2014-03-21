using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// throw文。
	/// The Throw statement.
    /// </summary>
    public class ThrowStatement : Statement
    {
		readonly Expression expr;

        /// <summary>
        /// throwする式。
		/// An expression which yields an exception.
        /// </summary>
        public Expression Expression{
			get{return expr;}
		}

		internal bool InFinally{get; set;}

        public override NodeType Type{
            get{return NodeType.ThrowStatement;}
        }

		public ThrowStatement(Expression expression)
		{
			expr = expression;
		}

        public override bool Equals(object obj)
        {
            var x = obj as ThrowStatement;

            if(x == null)
                return false;

            return expr.Equals(x.expr);
        }

        public override int GetHashCode()
        {
            return this.expr.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			var throwable = Expression.Run(varStore) as ExpressoObj;
			if(throwable == null)
				throw ExpressoOps.InvalidTypeError("The throw statement must throw a throwable object.");

			throw new ExpressoThrowException(throwable);
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(AstWalker walker)
		{
			if(walker.Walk(this))
				expr.Walk(walker);

			walker.PostWalk(this);
		}
    }
}
