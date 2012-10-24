﻿using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 二項演算。
    /// </summary>
    public class BinaryExpression : Expression
    {
        /// <summary>
        /// 演算子のタイプ。
        /// </summary>
        public OperatorType Operator { get; internal set; }

        /// <summary>
        /// 左辺のオペランド。
        /// </summary>
        public Expression Left { get; internal set; }

        /// <summary>
        /// 右辺のオペランド。
        /// </summary>
        public Expression Right { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.BinaryExpression; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as BinaryExpression;

            if (x == null) return false;

            return this.Operator == x.Operator
                && this.Left.Equals(x.Left)
                && this.Right.Equals(x.Right);
        }

        public override int GetHashCode()
        {
            return this.Operator.GetHashCode() ^ this.Left.GetHashCode() ^ this.Right.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
            ExpressoPrimitive first = Left.Run(varStore) as ExpressoPrimitive, second = Right.Run(varStore) as ExpressoPrimitive;
			if(first == null || second == null)
				throw new EvalException("Can not apply the operation to non-primitive types.");

			if((int)Operator <= (int)OperatorType.MOD){
				if(first.Value is int)
					return new ExpressoPrimitive{Value = BinaryExprAsInt((int)first.Value, (int)second.Value, Operator), Type = TYPES.INTEGER};
				else
					return new ExpressoPrimitive{Value = BinaryExprAsDouble((double)first.Value, (double)second.Value, Operator), Type = TYPES.FLOAT};
			}else if((int)Operator < (int)OperatorType.AND){
				return new ExpressoPrimitive{Value = EvalComparison(first.Value as IComparable, second.Value as IComparable, Operator), Type = TYPES.BOOL};
			}else if((int)Operator < (int)OperatorType.BIT_OR){
				return new ExpressoPrimitive{Value = EvalLogicalOperation((bool)first.Value, (bool)second.Value, Operator), Type = TYPES.BOOL};
			}else{
				return new ExpressoPrimitive{Value = EvalBitOperation((int)first.Value, (int)second.Value, Operator), Type = TYPES.INTEGER};
			}
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
				throw new EvalException("Unreachable code");
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
				throw new EvalException("Unreachable code");
			}
			
			return result;
		}
		
		private bool EvalComparison(IComparable lhs, IComparable rhs, OperatorType opType)
		{
			if(lhs == null || rhs == null)
				throw new EvalException("The operands can not be compared");
			
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
				throw new EvalException("Invalid Operation!");
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
