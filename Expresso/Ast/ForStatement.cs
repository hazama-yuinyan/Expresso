using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Expresso.Interpreter;
using Expresso.Builtins;
using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/**
	 * For文はインスタンス生成時に全てのメンバーが決定しないので、メンバーは全部変更可能にしておく。
	 */

	/// <summary>
	/// For文。
	/// The For statement.
	/// </summary>
	/// <seealso cref="BreakableStatement"/>
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
        /// body内で操作対象となるオブジェクトを参照するのに使用する式。
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
		public SequenceExpression Left{get; internal set;}

        /// <summary>
        /// 操作する対象の式。
        /// The target expression to be iterated over. It must yields a iterable object, otherwise a compile-time error
		/// (when compiling the code) or a runtime exception occurs(when in interpretering)
        /// </summary>
        public Expression Target{get; internal set;}

        /// <summary>
        /// 操作対象のオブジェクトが存在する間評価し続けるブロック。
        /// The block we'll continue to evaluate until the sequence is ate up.
        /// </summary>
        public Statement Body{get; internal set;}

        public override NodeType Type{
            get{return NodeType.ForStatement;}
        }

        public override bool Equals(object obj)
        {
            var x = obj as ForStatement;

            if(x == null)
                return false;
			
			return Left.Equals(x.Left) && Target.Equals(x.Target) && Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Left.GetHashCode() ^ this.Target.GetHashCode() ^ this.Body.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
            object iterable = Target.Run(varStore);
			if(!(iterable is IEnumerable))
				throw ExpressoOps.InvalidTypeError("Can not evaluate the expression to an iterable object!");

			can_continue = true;

			Identifier[] lvalues = new Identifier[LValues.Count];
			for(int i = 0; i < LValues.Count; ++i){
				lvalues[i] = LValues[i] as Identifier;
				if(lvalues[i] == null)
					throw ExpressoOps.ReferenceError("The left-hand-side of the \"in\" keyword must be a lvalue(a referencible value such as variables)");
			}

			var rvalue = ExpressoOps.Enumerate(iterable);
			while(can_continue && rvalue.MoveNext()){
				for(int j = 0; j < lvalues.Length; ++j){
					var lvalue = lvalues[j];
					varStore.Assign(lvalue.Level, lvalue.Offset, rvalue.Current);
					if(j + 1 != lvalues.Length){
						if(!rvalue.MoveNext())
							throw ExpressoOps.RuntimeError("The number of rvalues must be some multiply of that of lvalues.");
					}
				}

				Body.Run(varStore);
			}
			return null;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				Left.Walk(walker);
				Target.Walk(walker);
				Body.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			IEnumerable<Identifier> in_cond = Enumerable.Empty<Identifier>();
			/*if(HasLet){
				in_cond =
					from p in Left
					select ImplementationHelpers.CollectLocalVars(p) into t
					from q in t
					select q;
			}*/
			
			var in_body = ImplementationHelpers.CollectLocalVars(Body);
			return in_cond.Concat(in_body);
		}
	}
}

