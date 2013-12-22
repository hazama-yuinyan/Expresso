using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// 整数の数列をあらわす式.
	/// Represents an integer sequence.
	/// </summary>
	public class IntSeqExpression : Expression
	{
		Expression lower, upper, step;

		/// <summary>
		/// 整数列の下限.
		/// The lower bound of the integer sequence.
		/// The expression has to yield an integer.
		/// </summary>
		public Expression Lower{
			get{return lower;}
		}

		/// <summary>
		/// 整数列の上限.
		/// The upper bound of the integer sequence.
		/// The expression has to yield an integer.
		/// </summary>
		public Expression Upper{
			get{return upper;}
		}
		
		/// <summary>
		/// ステップ.
		/// The step by which an iteration proceeds at a time.
		/// The expression has to yield an integer.
		/// </summary>
		public Expression Step{
			get{return step;}
		}

		public IntSeqExpression(Expression start, Expression end, Expression stepExpr)
		{
			lower = start;
			upper = end;
			step = stepExpr;
		}
		
        public override NodeType Type{
            get{return NodeType.IntSequence;}
        }

        public override bool Equals(object obj)
        {
            var x = obj as IntSeqExpression;

            if(x == null)
                return false;

            return this.Lower == x.Lower && this.Upper == x.Upper && this.Step == x.Step;
        }

        public override int GetHashCode()
        {
            return this.Lower.GetHashCode() ^ this.Upper.GetHashCode() ^ this.Step.GetHashCode();
        }
		
		/*internal override object Run(VariableStore varStore)
		{
			var start = Start.Run(varStore);
			if(!(start is int))
				throw ExpressoOps.InvalidTypeError("The start expression of the IntSeq expression must yield an integer!");

			var end = End.Run(varStore);
			if(!(end is int))
				throw ExpressoOps.InvalidTypeError("The end expression of the IntSeq expression must yield an integer!");

			var step = Step.Run(varStore);
			if(!(step is int))
				throw ExpressoOps.InvalidTypeError("The step expression of the IntSeq expression must yield an integer!");

			return new ExpressoIntegerSequence((int)start, (int)end, (int)step);
		}*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				lower.Walk(walker);
				upper.Walk(walker);
				step.Walk(walker);
			}
			walker.PostWalk(this);
		}
	}
}

