using System.Collections.Generic;
using System.Linq;

using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// キャスト式。
	/// The cast expression.
    /// </summary>
    public class CastExpression : Expression
    {
		private readonly Expression to_expr;
		private readonly Expression target;

        /// <summary>
        /// キャスト先の型。
		/// The target type to which the expression casts the object.
        /// </summary>
        public Expression ToExpression{
			get{return to_expr;}
		}

        /// <summary>
        /// キャストを実行するオブジェクト。
		/// The target object to be casted.
        /// </summary>
        public Expression Target{
			get{return target;}
		}

        public override NodeType Type
        {
            get { return NodeType.CastExpression; }
        }

		public CastExpression(Expression toExpr, Expression targetExpr)
		{
			to_expr = toExpr;
			target = targetExpr;
		}

        public override bool Equals(object obj)
        {
            var x = obj as CastExpression;

            if (x == null) return false;

            return this.to_expr == x.to_expr && this.target.Equals(x.target);
        }

        public override int GetHashCode()
        {
            return this.to_expr.GetHashCode() ^ this.target.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
            return null;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				to_expr.Walk(walker);
				target.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return string.Format("(Cast){0} => {1}", target, to_expr);
		}
    }
}
