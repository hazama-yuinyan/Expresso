using System.Collections.Generic;

namespace Expresso.Ast
{
    /// <summary>
    /// 関数呼び出し。
    /// </summary>
    public class Call : Expression
    {
        /// <summary>
        /// 呼び出す対象。
        /// </summary>
        public Function Function { get; internal set; }

        /// <summary>
        /// 呼び出す対象の関数名。
        /// </summary>
        public string Name { get { return this.Function.Name; } }

        /// <summary>
        /// 与える実引数リスト。
        /// </summary>
        public List<Expression> Arguments { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.Call; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Call;

            if (x == null) return false;

            if (this.Name != x.Name) return false;

            if (this.Arguments.Count != x.Arguments.Count) return false;

            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (!this.Arguments[i].Equals(x.Arguments[i])) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Arguments.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            return null;
        }
    }
}
