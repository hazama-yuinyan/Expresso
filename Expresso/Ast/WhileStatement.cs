using System;
using System.Collections.Generic;

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

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            return null;
        }
	}
}

