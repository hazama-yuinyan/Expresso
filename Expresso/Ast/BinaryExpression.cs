using System.Collections.Generic;
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

        protected internal override object Run(VariableStore localTable, Scope functions)
        {
            foreach (var instruction in this.Left.Compile(localTable, addressTable, functionTable))
            {
                yield return instruction;
            }

            foreach (var instruction in this.Right.Compile(localTable, addressTable, functionTable))
            {
                yield return instruction;
            }

            switch (this.Operator)
            {
			case OperatorType.PLUS: yield return Expresso.Emulator.Instruction.Add(); break;
			case OperatorType.MINUS: yield return Expresso.Emulator.Instruction.Subtract(); break;
			case OperatorType.TIMES: yield return Expresso.Emulator.Instruction.Multiply(); break;
			case OperatorType.DIV: yield return Expresso.Emulator.Instruction.Divide(); break;
			case OperatorType.LESS: yield return Expresso.Emulator.Instruction.LessThan(); break;
			case OperatorType.LESE: yield return Expresso.Emulator.Instruction.LessEqual(); break;
			case OperatorType.GREAT: yield return Expresso.Emulator.Instruction.GreaterThan(); break;
			case OperatorType.GRTE: yield return Expresso.Emulator.Instruction.GreaterEqual(); break;
			case OperatorType.EQUAL: yield return Expresso.Emulator.Instruction.Equal(); break;
			case OperatorType.NOTEQ: yield return Expresso.Emulator.Instruction.NotEqual(); break;
			case OperatorType.AND: yield return Expresso.Emulator.Instruction.And(); break;
			case OperatorType.OR: yield return Expresso.Emulator.Instruction.Or(); break;
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
			return string.Format("[BinaryExpression: {1} {0} {2}]", op, Left, Right);
		}
    }
}
