using System.Collections.Generic;
using System.Linq;

namespace Expresso.Ast
{
    /// <summary>
    /// 条件演算。
    /// </summary>
    public class ConditionalExpression : Expression
    {
        /// <summary>
        /// 条件式。
        /// </summary>
        public Expression Condition { get; internal set; }

        /// <summary>
        /// 条件が真の時に返す式。
        /// </summary>
        public Expression TrueExpression { get; internal set; }

        /// <summary>
        /// 条件が偽の時に返す式。
        /// </summary>
        public Expression FalseExpression { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.ConditionalExpression; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ConditionalExpression;

            if (x == null) return false;

            return this.Condition == x.Condition
                && this.TrueExpression.Equals(x.TrueExpression)
                && this.FalseExpression.Equals(x.FalseExpression);
        }

        public override int GetHashCode()
        {
            return this.Condition.GetHashCode() ^ this.TrueExpression.GetHashCode() ^ this.FalseExpression.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            foreach (var instruction in this.Condition.Compile(localTable, addressTable, functionTable))
            {
                yield return instruction;
            }

            var t = this.TrueExpression.Compile(localTable, addressTable, functionTable);
            var f = this.FalseExpression.Compile(localTable, addressTable, functionTable);

            yield return Expresso.Emulator.Instruction.Branch(f.Count() + 1);

            foreach (var instruction in f)
            {
                yield return instruction;
            }

            yield return Expresso.Emulator.Instruction.Jump(t.Count());

            foreach (var instruction in t)
            {
                yield return instruction;
            }
        }
    }
}
