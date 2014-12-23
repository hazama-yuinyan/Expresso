using System.Collections.Generic;

using Expresso.Compiler;

namespace Expresso.Ast
{
    /// <summary>
    /// throw文。
	/// The Throw statement.
    /// </summary>
    public class ThrowStatement : Statement
    {
        /// <summary>
        /// throwする式。
		/// An expression which yields an exception.
        /// </summary>
        public Expression Expression{
            get{return GetChildByRole(Roles.Expression);}
		}

		internal bool InFinally{get; set;}

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

		public ThrowStatement(Expression expression)
		{
            AddChild(expression, Roles.Expression);
		}

        public override bool Equals(object obj)
        {
            var x = obj as ThrowStatement;

            if(x == null)
                return false;

            return this.Expression.Equals(x.Expression);
        }

        public override int GetHashCode()
        {
            return this.Expression.GetHashCode();
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitThrowStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitThrowStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitThrowStatement(this, data);
        }
    }
}
