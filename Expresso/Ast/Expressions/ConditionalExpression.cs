using System.Collections.Generic;
using System.Linq;

using Expresso.Compiler;

namespace Expresso.Ast
{
    /// <summary>
    /// 条件演算。
	/// The conditional expression.
    /// </summary>
    public class ConditionalExpression : Expression
    {
        /// <summary>
        /// 条件式。
		/// The condition expression to be tested.
        /// </summary>
        public Expression Condition{
            get{return (Expression)FirstChild;}
		}

        /// <summary>
        /// 条件が真の時に返す式。
		/// The expression to be evaluated when the condition is true.
        /// </summary>
        public Expression TrueExpression{
            get{return (Expression)FirstChild.NextSibling;}
		}

        /// <summary>
        /// 条件が偽の時に返す式。
		/// The expression to be evaluated when the condition is false.
        /// </summary>
        public Expression FalseExpression{
            get{return (Expression)LastChild;}
		}

        public override NodeType NodeType{
            get{return NodeType.ConditionalExpression;}
        }

		public ConditionalExpression(Expression test, Expression trueExpr, Expression falseExpr)
		{
            AddChild(test);
            AddChild(trueExpr);
            AddChild(falseExpr);
		}

        public override bool Equals(object obj)
        {
            var x = obj as ConditionalExpression;

            if(x == null)
                return false;

            return this.Condition == x.Condition
                && this.TrueExpression.Equals(x.TrueExpression)
                && this.FalseExpression.Equals(x.FalseExpression);
        }

        public override int GetHashCode()
        {
            return this.Condition.GetHashCode() ^ this.TrueExpression.GetHashCode() ^ this.FalseExpression.GetHashCode();
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitConditionalExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitConditionalExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitConditionalExpression(this, data);
        }

        public override string GetText()
		{
            return string.Format("{0} ? {1} : {2}", Condition, TrueExpression, FalseExpression);
		}

        public override string ToString()
        {
            return GetText();
        }
    }
}
