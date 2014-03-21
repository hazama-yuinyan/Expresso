using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// 単項演算。
	/// Reperesents an unary expression.
	/// </summary>
	public class UnaryExpression : Expression
	{
		readonly OperatorType ope;
		readonly Expression operand;

		/// <summary>
		/// 演算子のタイプ。
		/// The type of the operation.
		/// </summary>
		public OperatorType Operator{
			get{return ope;}
		}

		/// <summary>
		/// オペランド。
		/// The operand.
		/// </summary>
		public Expression Operand{
			get{return operand;}
		}

		public override NodeType Type{
			get{return NodeType.UnaryExpression;}
		}

		public UnaryExpression(OperatorType opType, Expression target)
		{
			ope = opType;
			operand = target;
		}

		public override bool Equals(object obj)
		{
			var x = obj as UnaryExpression;

			if(x == null)
				return false;

			return ope == x.ope && operand.Equals(x.operand);
		}

		public override int GetHashCode()
		{
			return ope.GetHashCode() ^ operand.GetHashCode();
		}

		/*internal override object Run(VariableStore varStore)
		{
			var ope = Operand.Run(varStore);
			if(ope == null)
				throw ExpressoOps.InvalidTypeError("Invalid object type!");
			
			if(Operator == OperatorType.MINUS){
				if(ope is int)
					ope = -(int)ope;
				else if(ope is double)
					ope = -(double)ope;
				else if(ope is Fraction)
					ope = -(Fraction)ope;
				else
					throw ExpressoOps.InvalidTypeError("The minus operator is not applicable to the operand!");
			}else if(Operator == OperatorType.NOT){
				if(ope is bool)
					ope = !(bool)ope;
				else
					throw ExpressoOps.InvalidTypeError("The not operator is not applicable to the operand!");
			}
			
			return ope;
		}*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(AstWalker walker)
		{
			if(walker.Walk(this))
				operand.Walk(walker);

			walker.PostWalk(this);
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
			return string.Format("{0}{1}", op, operand);
		}
	}
}
