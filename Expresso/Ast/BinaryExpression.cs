using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// 二項演算。
	/// Represents a binary expression.
    /// </summary>
    public class BinaryExpression : Expression
    {
		private readonly OperatorType ope;
		private readonly Expression lhs;
		private readonly Expression rhs;

        /// <summary>
        /// 演算子のタイプ。
		/// The type of the operator.
        /// </summary>
        public OperatorType Operator{
			get{return ope;}
		}

        /// <summary>
        /// 左辺のオペランド。
		/// The left operand.
        /// </summary>
        public Expression Left{
			get{return lhs;}
		}

        /// <summary>
        /// 右辺のオペランド。
		/// The right operand.
        /// </summary>
        public Expression Right{
			get{return rhs;}
		}

        public override NodeType Type
        {
            get { return NodeType.BinaryExpression; }
        }

		public BinaryExpression(Expression left, Expression right, OperatorType opType)
		{
			ope = opType;
			lhs = left;
			rhs = right;
		}

        public override bool Equals(object obj)
        {
            var x = obj as BinaryExpression;

            if (x == null) return false;

            return this.ope == x.ope
                && this.rhs.Equals(x.lhs)
                && this.rhs.Equals(x.rhs);
        }

        public override int GetHashCode()
        {
            return this.ope.GetHashCode() ^ this.lhs.GetHashCode() ^ this.rhs.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
            object first = Left.Run(varStore), second = Right.Run(varStore);
			if(first == null || second == null)
				throw ExpressoOps.InvalidTypeError("Can not apply the operation on null objects.");

			if((int)Operator <= (int)OperatorType.MOD){
				if(first is int)
					return BinaryExprAsInt((int)first, (int)second, Operator);
				else if(first is double)
					return BinaryExprAsDouble((double)first, (double)second, Operator);
				else if(first is Fraction)
					return BinaryExprAsFraction((Fraction)first, second, Operator);
				else
					return BinaryExprAsString((string)first, second, Operator);
			}else if((int)Operator < (int)OperatorType.AND){
				return EvalComparison(first as IComparable, second as IComparable, Operator);
			}else if((int)Operator < (int)OperatorType.BIT_OR){
				return EvalLogicalOperation((bool)first, (bool)second, Operator);
			}else{
				return EvalBitOperation((int)first, (int)second, Operator);
			}
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				Left.Walk(walker);
				Right.Walk(walker);
			}
			walker.PostWalk(this);
		}
		
		private int BinaryExprAsInt(int lhs, int rhs, OperatorType opType)
		{
			int result;
			
			switch (opType) {
			case OperatorType.PLUS:
				result = lhs + rhs;
				break;
				
			case OperatorType.MINUS:
				result = lhs - rhs;
				break;
				
			case OperatorType.TIMES:
				result = lhs * rhs;
				break;
				
			case OperatorType.DIV:
				result = lhs / rhs;
				break;
				
			case OperatorType.POWER:
				result = (int)Math.Pow(lhs, rhs);
				break;
				
			case OperatorType.MOD:
				result = lhs % rhs;
				break;
				
			default:
				throw ExpressoOps.RuntimeError("Internal Error: Unreachable code");
			}
			
			return result;
		}
		
		private double BinaryExprAsDouble(double lhs, double rhs, OperatorType opType)
		{
			double result;
			
			switch (opType) {
			case OperatorType.PLUS:
				result = lhs + rhs;
				break;
				
			case OperatorType.MINUS:
				result = lhs - rhs;
				break;
				
			case OperatorType.TIMES:
				result = lhs * rhs;
				break;
				
			case OperatorType.DIV:
				result = lhs / rhs;
				break;
				
			case OperatorType.POWER:
				result = Math.Pow(lhs, rhs);
				break;
				
			case OperatorType.MOD:
				result = Math.IEEERemainder(lhs, rhs);
				break;
				
			default:
				throw ExpressoOps.RuntimeError("Internal Error: Unreachable code");
			}
			
			return result;
		}

		private Fraction BinaryExprAsFraction(Fraction lhs, object rhs, OperatorType opType)
		{
			if(!(rhs is Fraction) && !(rhs is long) && !(rhs is int) && !(rhs is double))
				throw ExpressoOps.InvalidTypeError("The right operand have to be either a long, int, double or fraction!");

			Fraction result;

			switch(opType){
			case OperatorType.PLUS:
				result = lhs + rhs;
				break;

			case OperatorType.MINUS:
				result = lhs - rhs;
				break;

			case OperatorType.TIMES:
				result = lhs * rhs;
				break;

			case OperatorType.DIV:
				result = lhs / rhs;
				break;

			case OperatorType.POWER:
				result = lhs.Power(rhs);
				break;

			case OperatorType.MOD:
				result = lhs % rhs;
				break;

			default:
				throw ExpressoOps.RuntimeError("Internal Error: Unreachable code");
			}

			return result;
		}

		private string BinaryExprAsString(string lhs, object rhs, OperatorType opType)
		{
			string result;

			switch(opType){
			case OperatorType.PLUS:
				result = String.Concat(lhs, rhs.ToString());
				break;

			case OperatorType.TIMES:
				if(!(rhs is int))
					throw ExpressoOps.InvalidTypeError("Can not muliply string by objects other than an integer.");

				int times = (int)rhs;
				var sb = new StringBuilder(lhs.Length * times);
				for(; times > 0; --times) sb.Append(lhs);

				result = sb.ToString();
				break;

			default:
				throw ExpressoOps.RuntimeError("Strings don't support that operation!");
			}

			return result;
		}
		
		private bool EvalComparison(IComparable lhs, IComparable rhs, OperatorType opType)
		{
			if(lhs == null || rhs == null)
				throw ExpressoOps.InvalidTypeError("The operands can not be compared");
			
			switch (opType) {
			case OperatorType.EQUAL:
				return object.Equals(lhs, rhs);
				
			case OperatorType.GREAT:
				return lhs.CompareTo(rhs) > 0;
				
			case OperatorType.GRTE:
				return lhs.CompareTo(rhs) >= 0;
				
			case OperatorType.LESE:
				return lhs.CompareTo(rhs) <= 0;
				
			case OperatorType.LESS:
				return lhs.CompareTo(rhs) < 0;
				
			case OperatorType.NOTEQ:
				return !object.Equals(lhs, rhs);
				
			default:
				return false;
			}
		}

		private bool EvalLogicalOperation(bool lhs, bool rhs, OperatorType opType)
		{
			switch (opType) {
			case OperatorType.AND:
				return lhs && rhs;

			case OperatorType.OR:
				return lhs || rhs;

			default:
				return false;
			}
		}

		private int EvalBitOperation(int lhs, int rhs, OperatorType opType)
		{
			switch (opType) {
			case OperatorType.BIT_AND:
				return lhs & rhs;

			case OperatorType.BIT_XOR:
				return lhs ^ rhs;

			case OperatorType.BIT_OR:
				return lhs | rhs;

			case OperatorType.BIT_LSHIFT:
				return lhs << rhs;

			case OperatorType.BIT_RSHIFT:
				return lhs >> rhs;

			default:
				throw ExpressoOps.RuntimeError("Invalid Operation!");
			}
		}
		
		public override string ToString()
		{
			string op;
			switch (Operator) {
			case OperatorType.AND:
				op = "and";
				break;
				
			case OperatorType.DIV:
				op = "/";
				break;
				
			case OperatorType.EQUAL:
				op = "==";
				break;
				
			case OperatorType.GREAT:
				op = ">";
				break;
				
			case OperatorType.GRTE:
				op = ">=";
				break;
				
			case OperatorType.LESE:
				op = "<=";
				break;
				
			case OperatorType.LESS:
				op = "<";
				break;
				
			case OperatorType.MINUS:
				op = "-";
				break;
				
			case OperatorType.MOD:
				op = "%";
				break;

			case OperatorType.NOTEQ:
				op = "!=";
				break;
				
			case OperatorType.OR:
				op = "or";
				break;
				
			case OperatorType.PLUS:
				op = "+";
				break;
				
			case OperatorType.POWER:
				op = "**";
				break;
				
			case OperatorType.TIMES:
				op = "*";
				break;
				
			default:
				op = "";
				break;
			}
			return string.Format("{1} {0} {2}", op, Left, Right);
		}
    }
}
