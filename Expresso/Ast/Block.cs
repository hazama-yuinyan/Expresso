using System.Collections.Generic;
using System.Linq;
using Expresso.Interpreter;

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
		/// このブロックの親のブロック。
		/// </summary>
		public Statement Parent{get; internal set;}

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

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            object result = null;
			
			foreach (var stmt in Statements) {
				result = stmt.Run(varStore, funcTable);
			}
			
			return result;
        }
		
		public override string ToString()
		{
			return string.Format("[Block: Statements={0}, LocalVariables={1}, Type={2}]", Statements, LocalVariables, Type);
		}
    }
}
