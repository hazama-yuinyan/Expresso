using System;

using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// 実行時にならないと実際にひもづけられる変数がわからない場合に使用する。
	/// Helper node for late binding. Used to cache the result of binding at runtime.
	/// </summary>
	public class LateBindExpression<T> : Expression
		where T : class
	{
		readonly Expression target;

		readonly Interpreter.Interpreter interp;

		bool is_resolved = false;

		T bound_obj = default(T);

		public T Target{
			get{
				if(!is_resolved){
					bound_obj = interp.Interpret(target, null) as T;
					if(bound_obj == null)
						throw ExpressoOps.RuntimeError("{0} is not evaluated to an object of type {1}", target, typeof(T).FullName);

					is_resolved = true;
				}

				return bound_obj;
			}
		}

		public override NodeType Type {
			get {
				return NodeType.LateBind;
			}
		}

		public LateBindExpression(Expression expr, Interpreter.Interpreter interpreter)
		{
			target = expr;
			interp = interpreter;
		}

		public override string ToString()
		{
			return string.Format("<LateBind: {0}>", target);
		}

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			throw new System.NotImplementedException();
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				target.Walk(walker);
			}
		}
	}
}

