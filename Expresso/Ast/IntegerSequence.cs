using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// 整数の数列をあらわす式
	/// </summary>
	public class IntSeqExpression : Expression
	{
		/// <summary>
		/// 整数列の下限
		/// </summary>
		public int Start{get; internal set;}

		/// <summary>
		/// 整数列の上限
		/// </summary>
		public int End{get; internal set;}
		
		/// <summary>
		/// ステップ
		/// </summary>
		public int Step{get; internal set;}
		
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
			return new ExpressoIntegerSequence(Start, End, Step);
		}
	}
}

