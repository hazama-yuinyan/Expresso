using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Expresso.Interpreter;
using Expresso.Builtins;
using Expresso.Helpers;
using Expresso.Compiler;

namespace Expresso.Ast
{
	/// <summary>
	/// For文。
	/// The For statement.
	/// </summary>
	public class ForStatement : BreakableStatement, CompoundStatement
	{
		/// <summary>
		/// inキーワードの左辺値がlet式を含むかどうかをあらわす。
		/// Indicates whether the left-hand-side of the in keyword has a let keyword.
		/// Used to determine whether it first initializes local variables defined in the target expression
		/// of the for statement or not.
		/// </summary>
		public bool HasLet{get; internal set;}

		/// <summary>
        /// body内で操作対象となるオブジェクトを参照するのに使用する式群。
        /// 評価結果はlvalueにならなければならない。
        /// なお、走査の対象を捕捉する際には普通の代入と同じルールが適用される。
        /// つまり、複数の変数にいっせいにオブジェクトを捕捉させることもできる。
        /// When evaluating the both sides of the "in" keyword,
        /// the same rule as the assignment applies.
        /// So for example,
        /// for(let x, y in [1,2,3,4,5,6])...
        /// the x and y captures the first and second element of the list at the first time,
        /// the third and forth the next time, and the fifth and sixth at last.
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

        internal override object Run(VariableStore varStore)
        {
            IEnumerable iterable = Target.Run(varStore) as IEnumerable;
			if(iterable == null)
				throw new EvalException("Can not evaluate the expression to an iterable object!");

			can_continue = true;

			Identifier[] lvalues = new Identifier[LValues.Count];
			for (int i = 0; i < LValues.Count; ++i) {
				lvalues[i] = LValues[i] as Identifier;
				if(lvalues[i] == null)
					throw new EvalException("The left-hand-side of the \"in\" keyword must yield a lvalue(a referencible value such as variables)");
			}

			var enumerator = iterable.GetEnumerator();
			while (can_continue && enumerator.MoveNext()) {
				foreach (var lvalue in lvalues) {
					var val = enumerator.Current;
					varStore.Assign(lvalue.Level, lvalue.Offset, val);
				}

				Body.Run(varStore);
			}
			return null;
        }

		internal override System.Linq.Expressions.Expression Compile(Emitter emitter)
		{
			return emitter.Emit(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			IEnumerable<Identifier> in_cond = Enumerable.Empty<Identifier>();
			if(HasLet){
				in_cond =
					from p in LValues
					select ImplementationHelpers.CollectLocalVars(p) into t
					from q in t
					select q;
			}
			
			var in_body = ImplementationHelpers.CollectLocalVars(Body);
			return in_cond.Concat(in_body);
		}
	}
}

