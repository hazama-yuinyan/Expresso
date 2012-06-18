using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Helpers;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// If文。
	/// </summary>
	public class IfStatement : Statement
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

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            var cond = Condition.Run(varStore, funcTable) as ExpressoPrimitive;
			if(!ImplementaionHelpers.IsOfType(cond, TYPES.BOOL))
				throw new EvalException("Invalid expression! The condition of an if statement must yields a boolean!");
			
			if((bool)cond.Value)
				return TrueBlock.Run(new VariableStore{Parent = varStore}, funcTable);
			else
				return FalseBlock.Run(new VariableStore{Parent = varStore}, funcTable);
        }
	}
}

