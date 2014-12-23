using System.Collections.Generic;

using Expresso.Runtime;
using Expresso.Compiler;

namespace Expresso.Ast
{
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

		public override NodeType NodeType{
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

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitUnaryExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitUnaryExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitUnaryExpression(this, data);
        }

		public override string ToString()
		{
			string op;
			switch(Operator){
            case OperatorType.Plus:
				op = "+";
				break;

            case OperatorType.Minus:
				op = "-";
				break;

			case OperatorType.Not:
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
