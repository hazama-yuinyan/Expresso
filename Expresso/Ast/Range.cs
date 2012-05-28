using System.Collections.Generic;

namespace Expresso.Ast
{
	/// <summary>
	/// 整数の範囲をあらわす式
	/// </summary>
	public class RangeExpression : Expression
	{
		/// <summary>
		/// 範囲の下限
		/// </summary>
		public int Start{get; internal set;}
		
		/// <summary>
		/// 範囲の上限
		/// </summary>
		public int End{get; internal set;}
		
		/// <summary>
		/// ステップ
		/// </summary>
		public int Step{get; internal set;}
		
		public override NodeType Type
        {
            get { return NodeType.Range; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as RangeExpression;

            if (x == null) return false;

            return this.Start == x.Start && this.End == x.End && this.Step == x.Step;
        }

        public override int GetHashCode()
        {
            return this.Start.GetHashCode() ^ this.End.GetHashCode() ^ this.Step.GetHashCode();
        }
		
		protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
		{
			return null;
		}
	}
}

