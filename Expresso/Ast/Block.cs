using System.Collections.Generic;
using System.Linq;
using Expresso.Interpreter;
using Expresso.Helpers;

namespace Expresso.Ast
{
    /// <summary>
    /// 複文ブロック。
	/// Represents a block of statements.
    /// </summary>
    public class Block : BreakableStatement
    {
        private List<Statement> statements = new List<Statement>();

        /// <summary>
        /// ブロックの中身の文。
		/// The body statements
        /// </summary>
        public List<Statement> Statements { get { return this.statements; } }

        /// <summary>
        /// ブロック中で定義された変数一覧。
		/// The local variables defined in the block.
        /// </summary>
        public IEnumerable<Identifier> LocalVariables
        {
            get
            {
                return ImplementationHelpers.CollectLocalVars(this);
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

        internal override object Run(VariableStore varStore)
        {
			object result = null;
			can_continue = true;
			
			for(int i = 0; can_continue && i < statements.Count; ++i)
				result = statements[i].Run(varStore);
			
			return result;
        }
		
		public override string ToString()
		{
			return string.Format("[Statements.length={0}, ({1})]", Statements.Count, LocalVariables);
		}
    }
}
