using System;
using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Helpers;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// While文.
	/// The While statement.
	/// </summary>
	/// <seealso cref="Node"/>
	/// <seealso cref="BreakableStatement"/>
	public class WhileStatement : BreakableStatement, CompoundStatement
	{
		/// <summary>
        /// 条件式。
		/// The condition.
        /// </summary>
        public Expression Condition { get; internal set; }

        /// <summary>
        /// 条件が真の間評価し続ける文(郡)。
		/// A statement or a block to be processed while the condition is true.
        /// </summary>
        public Statement Body { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.WhileStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as WhileStatement;

            if (x == null) return false;

            return this.Condition == x.Condition
                && this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Condition.GetHashCode() ^ this.Body.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			object cond = null;

			can_continue = true;

			try{
				while(can_continue && (cond = Condition.Run(varStore)) != null && (bool)cond){
					Body.Run(varStore);
				}
			}
			catch(Exception){
				if(!(cond is bool))
					throw new EvalException("Invalid expression! The condition of a while statement must yield a boolean!");
			}

			return null;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			return ImplementationHelpers.CollectLocalVars(Body);
		}
	}
}

