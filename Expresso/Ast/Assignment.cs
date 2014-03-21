using System.Collections.Generic;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;
using System.Linq;

namespace Expresso.Ast
{
    /// <summary>
    /// 代入文。
	/// The assignment statement.
	/// targets.length == 1 for simple assignments like "x = 1"
    /// and targets.length == 3 for more complex assignments like "x = y = z = 3"
    /// </summary>
    public class Assignment : Statement
    {
        int num_left;

        /// <summary>
        /// 代入先の変数郡。
        /// The target expressions that will be bounded.
        /// </summary>
        public Expression[] Left{
            get{return Children.Take(num_left);}
		}

        /// <summary>
        /// 右辺値の式。
		/// The expression that will be assigned.
        /// </summary>
        public Expression Right{
            get{return LastChild;}
		}

        public override NodeType Type{
            get{return NodeType.Assignment;}
        }

		public Assignment(Expression[] lhs, Expression rhsExpr)
		{
            foreach(var left in lhs)
                AddChild(left);
			
            AddChild(rhsExpr);
		}

        public override bool Equals(object obj)
        {
            var x = obj as Assignment;

            if(x == null)
                return false;

            return this.Right.Equals(x.Right)
                && this.Left.Equals(x.Left);
        }

        public override int GetHashCode()
        {
            return this.Left.GetHashCode() ^ this.Right.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){
                foreach(var e in Left)
                    e.AcceptWalker(walker);

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
			var sb = new StringBuilder();
            foreach(var target in Left)
				sb.Append(target + " = ");

            sb.Append(Right);
			return sb.ToString();
		}

        public override string ToString()
        {
            return GetText();
        }
    }
}
