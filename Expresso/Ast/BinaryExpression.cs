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
    /// <summary>
    /// 二項演算。
	/// Represents a binary expression.
    /// </summary>
    public class BinaryExpression : Expression
    {
		readonly OperatorType ope;

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
            get{return (Expression)FirstChild;}
		}

        /// <summary>
        /// 右辺のオペランド。
		/// The right operand.
        /// </summary>
        public Expression Right{
            get{return (Expression)FirstChild.NextSibling;}
		}

        public override NodeType Type{
            get{return NodeType.BinaryExpression;}
        }

		public BinaryExpression(Expression left, Expression right, OperatorType opType)
		{
			ope = opType;
            AddChild(left);
            AddChild(right);
		}

        public override bool Equals(object obj)
        {
            var x = obj as BinaryExpression;

            if(x == null)
                return false;

            return this.ope == x.ope
                && this.Left.Equals(x.Left)
                && this.Right.Equals(x.Right);
        }

        public override int GetHashCode()
        {
            return this.ope.GetHashCode() ^ this.Left.GetHashCode() ^ this.Right.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){
                Left.AcceptWalker(walker);
                Right.AcceptWalker(walker);
			}
			walker.PostWalk(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
        }
		
        public override string GetText()
		{
			string op;
			switch(Operator){
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

        public override string ToString()
        {
            return GetText();
        }
    }
}
