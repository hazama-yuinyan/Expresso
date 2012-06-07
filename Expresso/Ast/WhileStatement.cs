using System;
using System.Collections.Generic;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// While文.
	/// </summary>/
	public class WhileStatement : Statement
	{
		/// <summary>
        /// 条件式。
        /// </summary>
        public Expression Condition { get; internal set; }

        /// <summary>
        /// 条件が真の間評価し続ける文(郡)。
        /// </summary>
        public Statement Body { get; internal set; }

		bool going_to_break = false;

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

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            Nullable<bool> cond;
			
			while((cond = Condition.Run(varStore, funcTable) as Nullable<bool>) != null && (bool)cond){
				Body.Run(varStore, funcTable);
			}
			
			if(cond == null)
				throw new EvalException("Invalid expression! The condition of a while statement must yields a boolean!");
			
			return null;
        }
	}
}

