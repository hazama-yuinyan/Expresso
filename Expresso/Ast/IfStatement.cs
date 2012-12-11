using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Builtins;
using Expresso.Helpers;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	/// <summary>
	/// If文。
	/// The If statement.
	/// </summary>
	public class IfStatement : Statement, CompoundStatement
	{
		/// <summary>
        /// 条件式。
        /// </summary>
        public Expression Condition { get; internal set; }

        /// <summary>
        /// 条件が真の時に評価する文(郡)。
        /// </summary>
        public Statement TrueBlock { get; internal set; }

        /// <summary>
        /// 条件が偽の時に評価する文(郡)。
        /// </summary>
        public Statement FalseBlock { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.IfStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as IfStatement;

            if (x == null) return false;

            return this.Condition == x.Condition
                && this.TrueBlock.Equals(x.TrueBlock)
                && this.FalseBlock.Equals(x.FalseBlock);
        }

        public override int GetHashCode()
        {
            return this.Condition.GetHashCode() ^ this.TrueBlock.GetHashCode() ^ this.FalseBlock.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
            var cond = Condition.Run(varStore);
			if(!(cond is bool))
				throw new EvalException("Invalid expression! The condition of an if statement must yields a boolean!");
			
			if((bool)cond)
				return TrueBlock.Run(varStore);
			else
				return (FalseBlock != null) ? FalseBlock.Run(varStore) : null;
        }

		internal override System.Linq.Expressions.Expression Compile(Emitter emitter)
		{
			return emitter.Emit(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			var in_true = ImplementationHelpers.CollectLocalVars(TrueBlock);
			var in_false = ImplementationHelpers.CollectLocalVars(FalseBlock);
			return in_true.Concat(in_false);
		}
	}
}

