using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;
using Expresso.Helpers;

namespace Expresso.Ast
{
	/// <summary>
	/// 単項演算。
	/// Reperesents an unary expression.
	/// </summary>
	public class UnaryExpression : Expression
	{
		/// <summary>
		/// 演算子のタイプ。
		/// </summary>
		public OperatorType Operator { get; internal set; }

		/// <summary>
		/// オペランド。
		/// The operand.
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
			var ope = Operand.Run(varStore, funcTable) as ExpressoPrimitive;
			if(ope == null)
				throw new EvalException("Invalid object type!");
			
			if(Operator == OperatorType.MINUS){
				if(ImplementaionHelpers.IsOfType(ope, TYPES.INTEGER))
					ope.Value = -(int)ope.Value;
				else if(ImplementaionHelpers.IsOfType(ope, TYPES.FLOAT))
					ope.Value = -(double)ope.Value;
				else
					throw new EvalException("The minus operator is not applicable to the operand!");
			}
			
			return ope;
		}
	}
}
