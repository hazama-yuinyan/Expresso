using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	/// <summary>
	/// 整数の数列をあらわす式.
	/// Represents an integer sequence.
	/// </summary>
	public class IntSeqExpression : Expression
	{
		/// <summary>
		/// 整数列の下限.
		/// The lower bound of the integer sequence.
		/// The expression has to yield an integer.
		/// </summary>
		public Expression Start{get; internal set;}

		/// <summary>
		/// 整数列の上限.
		/// The upper bound of the integer sequence.
		/// The expression has to yield an integer.
		/// </summary>
		public Expression End{get; internal set;}
		
		/// <summary>
		/// ステップ.
		/// The step by which an iteration proceeds at a time.
		/// The expression has to yield an integer.
		/// </summary>
		public Expression Step{get; internal set;}
		
		public override NodeType Type
        {
            get { return NodeType.Sequence; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as IntSeqExpression;

            if (x == null) return false;

            return this.Start == x.Start && this.End == x.End && this.Step == x.Step;
        }

        public override int GetHashCode()
        {
            return this.Start.GetHashCode() ^ this.End.GetHashCode() ^ this.Step.GetHashCode();
        }
		
		internal override object Run(VariableStore varStore)
		{
			var start = Start.Run(varStore);
			if(!(start is int))
				throw new EvalException("The start expression of the IntSeq expression must yield an integer!");

			var end = End.Run(varStore);
			if(!(end is int))
				throw new EvalException("The end expression of the IntSeq expression must yield an integer!");

			var step = Step.Run(varStore);
			if(!(step is int))
				throw new EvalException("The step expression of the IntSeq expression must yield an integer!");

			return new ExpressoIntegerSequence((int)start, (int)end, (int)step);
		}

		internal override System.Linq.Expressions.Expression Compile(Emitter emitter)
		{
			return emitter.Emit(this);
		}
	}
}

