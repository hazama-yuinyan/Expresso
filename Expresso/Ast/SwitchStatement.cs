using System;
using System.Collections.Generic;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// Switch文。
	/// </summary>
	public class SwitchStatement : Statement
	{
		/// <summary>
        /// 評価対象となる式。
        /// </summary>
        public Expression Target { get; internal set; }

        /// <summary>
        /// 分岐先となるラベル(郡)。
        /// </summary>
        public List<Statement> Labels { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.SwitchStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as SwitchStatement;

            if (x == null) return false;

            return this.Target == x.Target
                && this.Labels.Equals(x.Labels);
        }

        public override int GetHashCode()
        {
            return this.Target.GetHashCode() ^ this.Labels.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            /*Nullable<bool> cond;
			
			while((cond = Condition.Run(varStore, funcTable) as Nullable<bool>) != null && (bool)cond){
				Body.Run(varStore, funcTable);
			}
			
			if(cond == null)
				throw new EvalException("Invalid expression! The condition of a while statement must yields a boolean!");
			*/
			return null;
        }
	}
}

