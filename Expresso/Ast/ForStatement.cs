using System;
using System.Collections;
using System.Collections.Generic;
using Expresso.Interpreter;
using Expresso.BuiltIns;

namespace Expresso.Ast
{
	/// <summary>
	/// For文。
	/// </summary>
	public class ForStatement : Statement
	{
		/// <summary>
        /// body内で操作対象となるオブジェクトを参照するのに使用する式群。
        /// 評価結果はlvalueにならなければならない。
        /// なお、走査の対象を捕捉する際には普通の代入と同じルールが適用される。
        /// つまり、複数の変数にいっせいにオブジェクトを捕捉させることもできる。
        /// When evaluating the both sides of the "in" keyword,
        /// the same rule as the assignment applies.
        /// So for example,
        /// for(let $x, $y in [1,2,3,4,5,6])...
        /// the x and y captures the first and second element of the list at the first time,
        /// the third and forth the next time and the fifth and sixth at last.
        /// </summary>
        public List<Expression> LValues { get; internal set; }

        /// <summary>
        /// 操作する対象の式。
        /// The target expression.
        /// </summary>
        public Expression Target { get; internal set; }

        /// <summary>
        /// 操作対象のオブジェクトが存在する間評価し続けるブロック。
        /// The block we'll continue to evaluate until the sequence is ate up.
        /// </summary>
        public Statement Body { get; internal set; }

		bool going_to_break = false;

        public override NodeType Type
        {
            get { return NodeType.ForStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ForStatement;

            if (x == null) return false;
			
			return LValues.Equals(x.LValues) && Target.Equals(x.Target) && Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.LValues.GetHashCode() ^ this.Target.GetHashCode() ^ this.Body.GetHashCode();
        }

		/*public Statement GetParent()
		{
			var block = Body as Block;
			if(block == null)
				throw new EvalException("Something wrong has occurred when evalating the body of a for loop.");

			return block.Parent;
		}*/

		public void SetGoingToBreak()
		{
			going_to_break = true;
		}

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            IEnumerable iterable = Target.Run(varStore, funcTable) as IEnumerable;
			if(iterable == null)
				throw new EvalException("Can not evaluate the expression as a valid object");

			Parameter[] lvalues = new Parameter[LValues.Count];
			for (int i = 0; i < LValues.Count; ++i) {
				lvalues[i] = LValues[i] as Parameter;
				if(lvalues[i] == null)
					throw new EvalException("The left-hand-side of the \"in\" keyword must yield a lvalue(an referencible value such as variables)");
			}
			var enumerator = iterable.GetEnumerator();
			while (true) {
				foreach (var lvalue in lvalues) {
					var val = enumerator.Current;
					varStore.Assign(lvalue.Name, val);
				}
				if(going_to_break || !enumerator.MoveNext())
					break;

				Body.Run(varStore, funcTable);
			}
			return null;
        }
	}
}

