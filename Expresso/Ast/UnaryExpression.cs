using System.Collections.Generic;

namespace Expresso.Ast
{
	/// <summary>
	/// 単項演算。
	/// </summary>
	public class UnaryExpression : Expression
	{
		/// <summary>
		/// 演算子のタイプ。
		/// </summary>
		public OperatorType Operator { get; internal set; }

		/// <summary>
		/// オペランド。
		/// </summary>
		public Expression Operand { get; internal set; }

		public override NodeType Type {
			get { return NodeType.UnaryExpression; }
		}

		public override bool Equals (object obj)
		{
			var x = obj as UnaryExpression;

			if (x == null)
				return false;

			return this.Operator == x.Operator
                && this.Operand.Equals (x.Operand);
		}

		public override int GetHashCode ()
		{
			return this.Operator.GetHashCode () ^ this.Operand.GetHashCode ();
		}

		protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile (Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
		{
			return null;
		}
	}
}
