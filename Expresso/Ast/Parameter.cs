using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expresso.BuiltIns;

namespace Expresso.Ast
{
    /// <summary>
    /// 変数。
    /// </summary>
    public class Parameter : Expression
    {
        /// <summary>
        /// 変数名。
        /// </summary>
        public string Name { get; internal set; }
		
		public TYPES ParamType{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.Parameter; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Parameter;

            if (x == null) return false;

            return this.Name == x.Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            yield return Expresso.Emulator.Instruction.LoadLocal(localTable[this]);
        }
    }
}
