using System;

using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// Helper node for late binding. Used to cache the result of binding at runtime.
	/// </summary>
	public class LateBindExpression<T> : Expression
	{
		private readonly Expression target;

		private readonly Interpreter.Interpreter interp;

		private bool is_resolved = false;

		private T bound_obj = null;

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
			throw new System.NotImplementedException();
		}
	}
}

