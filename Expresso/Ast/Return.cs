using System.Collections.Generic;

namespace Expresso.Ast
{
    /// <summary>
    /// リターン文。
    /// </summary>
    public class Return : Statement
    {
        /// <summary>
        /// 戻り値の式。
        /// </summary>
        public List<Expression> Expressions { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.Return; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Return;

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
