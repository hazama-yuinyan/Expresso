using System.Collections.Generic;
using Expresso.Interpreter;

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

		internal override object Run(VariableStore varStore, Scope funcTable)
		{
			object ope = Operand.Run(varStore, funcTable);
			
			if(Operator == OperatorType.MINUS){
				if(ope is int)
					return -(int)ope;
				else if(ope is double)
					return -(double)ope;
				else
					throw new EvalException("The minus operator is not applicable to the operand!");
			}
			
			return null;
		}
	}
}
