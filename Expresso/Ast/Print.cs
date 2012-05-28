using System;
using System.Collections.Generic;

namespace Expresso.Ast
{
	public class PrintStatement : Statement
	{
		public Expression Expression{get; internal set;}
		
		public override NodeType Type
        {
            get { return NodeType.Print; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as PrintStatement;

            if (x == null) return false;

            return this.Expression.Equals(x.Expression);
        }

        public override int GetHashCode()
        {
            return this.Expression.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
			return null;
        }
	}
}

