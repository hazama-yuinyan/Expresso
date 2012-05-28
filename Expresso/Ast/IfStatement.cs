using System;
using System.Collections.Generic;

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

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            return null;
        }
	}
}

