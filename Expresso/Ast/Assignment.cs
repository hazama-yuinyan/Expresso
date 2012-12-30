using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

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

        internal override object Run(VariableStore varStore)
        {
			int i;
			var rvalues = new List<object>(Expressions.Count);
			for(i = 0; i < Expressions.Count; ++i)	//まず右辺をすべて評価する
				rvalues.Add(Expressions[i].Run(varStore));

			for(i = 0; i < Targets.Count; ++i){		//その後左辺値に代入する
				var assignable = Targets[i] as Assignable;
				if(assignable == null)
					throw ExpressoOps.ReferenceError("Can not assign a value to the target!");

				assignable.Assign(varStore, rvalues[i]);
			}
			return rvalues[0];	//x = y = 0;みたいな表記を許容するために右辺値の一番目を戻り値にする
        }
		
		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString()
		{
			return string.Format("{0} = {1}", Targets, Expressions);
		}
    }
}
