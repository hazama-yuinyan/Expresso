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
        /// <summary>
        /// yieldする式。
        /// </summary>
        public Expression Expression { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.YieldStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as YieldStatement;

            if (x == null) return false;

            return this.Expression.Equals(x.Expression);
        }

        public override int GetHashCode()
        {
            return this.Expression.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			return null;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}
    }
}
