using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Helpers;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// While文.
	/// The While statement.
	/// </summary>
	/// <seealso cref="Node"/>
	public class WhileStatement : BreakableStatement
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
			ExpressoPrimitive cond;

			can_continue = true;
			
			while(can_continue && (cond = Condition.Run(varStore) as ExpressoPrimitive) != null && (bool)cond.Value){
				Body.Run(varStore);
			}
			
			if(!ImplementaionHelpers.IsOfType(cond, TYPES.BOOL))
				throw new EvalException("Invalid expression! The condition of a while statement must yields a boolean!");
			
			return null;
        }
	}
}

