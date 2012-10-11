using System.Collections.Generic;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 関数呼び出し。
	/// Reperesents a function call.
    /// </summary>
    public class Call : Expression
    {
        /// <summary>
        /// 呼び出す対象。
		/// The target function to be called.
        /// </summary>
        public Function Function { get; internal set; }

        /// <summary>
        /// 呼び出す対象の関数名。
		/// The target function name.
        /// </summary>
        public string Name { get { return this.Function.Name; } }

        /// <summary>
        /// 与える実引数リスト。
		/// The argument list to be supplied to the call.
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

            for (int i = 0; i < this.Arguments.Count; ++i)
            {
                if (!this.Arguments[i].Equals(x.Arguments[i])) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Arguments.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            Function fn = Function;
			var child = new VariableStore{Parent = varStore};
			for (int i = 0; i < fn.Parameters.Count; ++i) {	//実引数をローカル変数として変数テーブルに追加する
				child.Add(fn.Parameters[i].Name, (Arguments.Count <= i) ? fn.Parameters[i].Option.Run(varStore, funcTable) : Arguments[i].Run(varStore, funcTable));
			}
			
			return Apply(fn, child, funcTable);
        }
		
		private object Apply(Function fn, VariableStore child, Scope funcTable)
		{
			return fn.Body.Run(child, funcTable);
		}
    }
}
