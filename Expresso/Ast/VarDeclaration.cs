using System;
using System.Collections.Generic;

namespace Expresso.Ast
{
	public class VarDeclaration : Statement
	{
		public List<Parameter> Variables { get; internal set; }

        /// <summary>
        /// 右辺値の式。
        /// </summary>
        public List<Expression> Expressions { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.VarDecl; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as VarDeclaration;

            if (x == null) return false;

            return this.Expressions.Equals(x.Expressions)
                && this.Variables.Equals(x.Variables);
        }

        public override int GetHashCode()
        {
            return this.Variables.GetHashCode() ^ this.Expressions.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
			return null;
        }
	}
}

