using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 代入文。
	/// The assignment statement.
    /// </summary>
    public class Assignment : Statement
    {
        /// <summary>
        /// 代入先の変数郡。
        /// The target expressions that will be bounded.
        /// </summary>
        public List<Expression> Targets { get; internal set; }

        /// <summary>
        /// 右辺値の式。
		/// The expressions that will be assigned.
        /// </summary>
        public List<Expression> Expressions { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.Assignment; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Assignment;

            if (x == null) return false;

            return this.Expressions.Equals(x.Expressions)
                && this.Targets.Equals(x.Targets);
        }

        public override int GetHashCode()
        {
            return this.Targets.GetHashCode() ^ this.Expressions.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
			int i;
			var rvalues = new List<ExpressoObj>();
			for(i = 0; i < Expressions.Count; ++i)	//まず右辺をすべて評価する
				rvalues.Add((ExpressoObj)Expressions[i].Run(varStore, funcTable));

			for (i = 0; i < Targets.Count; ++i) {	//その後左辺値に代入する
				Parameter lvalue = (Parameter)Targets[i];
				varStore.Assign(lvalue.Name, rvalues[i]);
			}
			return rvalues;
        }
		
		public override string ToString()
		{
			return string.Format("[Assignment: Variables={0}, Expressions={1}]", Targets.ToString(), Expressions.ToString());
		}
    }
}
