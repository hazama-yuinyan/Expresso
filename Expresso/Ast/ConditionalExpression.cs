using System.Collections.Generic;
using System.Linq;

using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// 条件演算。
	/// The conditional expression.
    /// </summary>
    public class ConditionalExpression : Expression
    {
		private readonly Expression condition;
		private readonly Expression true_expr;
		private readonly Expression false_expr;

        /// <summary>
        /// 条件式。
		/// The condition expression to be tested.
        /// </summary>
        public Expression Condition{
			get{return condition;}
		}

        /// <summary>
        /// 条件が真の時に返す式。
		/// The expression to be evaluated when the condition is true.
        /// </summary>
        public Expression TrueExpression{
			get{return true_expr;}
		}

        /// <summary>
        /// 条件が偽の時に返す式。
		/// The expression to be evaluated when the condition is false.
        /// </summary>
        public Expression FalseExpression{
			get{return false_expr;}
		}

        public override NodeType Type
        {
            get { return NodeType.ConditionalExpression; }
        }

		public ConditionalExpression(Expression test, Expression trueExpr, Expression falseExpr)
		{
			condition = test;
			true_expr = trueExpr;
			false_expr = falseExpr;
		}

        public override bool Equals(object obj)
        {
            var x = obj as ConditionalExpression;

            if (x == null) return false;

            return this.condition == x.condition
                && this.true_expr.Equals(x.true_expr)
                && this.false_expr.Equals(x.false_expr);
        }

        public override int GetHashCode()
        {
            return this.condition.GetHashCode() ^ this.true_expr.GetHashCode() ^ this.false_expr.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
            if((bool)Condition.Run(varStore))
				return TrueExpression.Run(varStore);
			else
				return FalseExpression.Run(varStore);
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				condition.Walk(walker);
				true_expr.Walk(walker);
				false_expr.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return string.Format("{0} ? {1} : {2}", condition, true_expr, false_expr);
		}
    }
}
