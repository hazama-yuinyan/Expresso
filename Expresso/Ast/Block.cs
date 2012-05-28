using System.Collections.Generic;
using System.Linq;

namespace Expresso.Ast
{
    /// <summary>
    /// 複文ブロック。
    /// </summary>
    public class Block : Statement
    {
        private List<Statement> statements = new List<Statement>();

        /// <summary>
        /// ブロックの中身の文。
        /// </summary>
        public List<Statement> Statements { get { return this.statements; } }

        /// <summary>
        /// ブロック中で定義された変数一覧。
        /// </summary>
        public IEnumerable<Parameter> LocalVariables
        {
            get
            {
                return this.Statements.OfType<VarDeclaration>().Select(x => x.Variables[0]);
            }
        }

        public override NodeType Type
        {
            get { return NodeType.Block; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Block;

            if (x == null) return false;

            if (this.Statements.Count != x.Statements.Count) return false;

            for (int i = 0; i < this.Statements.Count; i++)
            {
                if (!this.Statements[i].Equals(x.Statements[i])) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return this.Statements.GetHashCode();
        }

        static int Max(IEnumerable<int> values)
        {
            int max = 0;
            foreach (var v in values)
            {
                if (v > max) max = v;
            }
            return max;
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            var variables = this.LocalVariables;

            int nArgs = Max(localTable.Values);
            int i = nArgs;
            foreach (var v in variables)
            {
                localTable.Add(v, i);
                ++i;
            }

            if (i - nArgs != 0)
            {
                yield return Expresso.Emulator.Instruction.PrepareLocal(i - nArgs);
            }

            foreach (var s in this.Statements)
            {
                foreach (var instruction in s.Compile(localTable, addressTable, functionTable))
                {
                    yield return instruction;
                }
            }
        }
		
		public override string ToString()
		{
			return string.Format("[Block: Statements={0}, LocalVariables={1}, Type={2}]", Statements, LocalVariables, Type);
		}
    }
}
