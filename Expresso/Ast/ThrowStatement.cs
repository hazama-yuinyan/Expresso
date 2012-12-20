using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// throw文。
	/// The Throw statement.
    /// </summary>
    public class ThrowStatement : Statement
    {
        /// <summary>
        /// throwする式。
        /// </summary>
        public Expression Expression { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.ThrowStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ThrowStatement;

            if (x == null) return false;

            return this.Expression.Equals(x.Expression);
        }

        public override int GetHashCode()
        {
            return this.Expression.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			var throwable = Expression.Run(varStore) as ExpressoObj;
			if(throwable == null)
				throw new EvalException("The throw statement must throw a throwable object.");

			throw new ExpressoThrowException(throwable);
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}
    }
}
