using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// リターン文。
	/// The Return statement.
    /// </summary>
    public class ReturnStatement : Statement
    {
		readonly Expression expr;

        /// <summary>
        /// 戻り値の式。
		/// The expression generating the return value. It can be null if there is no return value.
        /// </summary>
        public Expression Expression{
			get{return expr;}
		}

        public override NodeType Type{
            get{return NodeType.Return;}
        }

		public ReturnStatement(Expression expression)
		{
			expr = expression;
		}

        public override bool Equals(object obj)
        {
            var x = obj as ReturnStatement;

            if(x == null)
                return false;

            return expr.Equals(x.expr);
        }

        public override int GetHashCode()
        {
            return this.expr.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
            if(Expressions.Count == 0){
				return new ExpressoTuple(new List<object>());
			}else if(Expressions.Count == 1){
				return Expressions[0].Run(varStore);
			}else{
				var objs = new List<object>();
				foreach (Expression expr in Expressions)
					objs.Add(expr.Run(varStore));
				
				return ExpressoOps.MakeTuple(objs);
			}
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(AstWalker walker)
		{
			if(walker.Walk(this)){
				if(expr != null)
					expr.Walk(walker);
			}
			walker.PostWalk(this);
		}
    }
}
