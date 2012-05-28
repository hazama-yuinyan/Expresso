using System;
using System.Collections.Generic;

namespace Expresso.Ast
{
	public class ExprStatement : Statement
	{
        /// <summary>
        /// 実行する式のリスト。
        /// </summary>
        public List<Expression> Expressions { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.ExprStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ExprStatement;

            if (x == null) return false;

            return this.Expressions.Equals(x.Expressions);
        }

        public override int GetHashCode()
        {
            return this.Expressions.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
			return null;
        }
	}
}

