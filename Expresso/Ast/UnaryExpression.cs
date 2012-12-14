using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Helpers;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

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

		public override bool Equals(object obj)
		{
			var x = obj as UnaryExpression;

			if (x == null)
				return false;

			return this.Operator == x.Operator
                && this.Operand.Equals(x.Operand);
		}

		public override int GetHashCode()
		{
			return this.Operator.GetHashCode() ^ this.Operand.GetHashCode();
		}

		internal override object Run(VariableStore varStore)
		{
			var ope = Operand.Run(varStore);
			if(ope == null)
				throw new EvalException("Invalid object type!");
			
			if(Operator == OperatorType.MINUS){
				if(ope is int)
					ope = -(int)ope;
				else if(ope is double)
					ope = -(double)ope;
				else if(ope is Fraction)
					ope = -(Fraction)ope;
				else
					throw new EvalException("The minus operator is not applicable to the operand!");
			}else if(Operator == OperatorType.NOT){
				if(ope is bool)
					ope = !(bool)ope;
				else
					throw new EvalException("The not operator is not applicable to the operand!");
			}
			
			return ope;
		}

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString()
		{
			string op;
			switch(Operator){
			case OperatorType.PLUS:
				op = "+";
				break;

			case OperatorType.MINUS:
				op = "-";
				break;

			case OperatorType.NOT:
				op = "!";
				break;

			default:
				op = "";
				break;
			}
			return string.Format("{0}{1}", op, Operand);
		}
	}
}
