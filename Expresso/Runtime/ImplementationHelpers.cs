using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using ExprTree = System.Linq.Expressions;

using Expresso.Ast;
using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Builtins.Library;
using Expresso.Runtime.Operations;
using Expresso.Compiler.Meta;

namespace Expresso.Runtime
{
	/// <summary>
	/// Expressoの実装用のヘルパー関数郡。
	/// Helper functions for implementing Expresso.
	/// </summary>
	public static class ImplementationHelpers
	{
		public static IEnumerable<Identifier> CollectLocalVars(Expression expr)
		{
			if(expr.Type == NodeType.VarDecl)
				return ((VarDeclaration)expr).Left;
			else if(expr.Type == NodeType.Identifier)
				return new Identifier[]{(Identifier)expr};
			else
				throw ExpressoOps.RuntimeError("Unreachable code reached!");
		}

		public static IEnumerable<Identifier> CollectLocalVars(Statement stmt)
		{
			var compound = stmt as CompoundStatement;
			if(stmt == null || compound == null) return Enumerable.Empty<Identifier>();

			return compound.CollectLocalVars();
		}

		/*public static Argument MakeArg(string name, TypeAnnotation type, Expression option = null)
		{
			return new Argument(name, option, new ExpressoVariable(name, type));
		}*/

		public static ExprTree.LambdaExpression MakeNativeCtorCall(Type targetType, params Type[] types)
		{
			var ctor = targetType.GetConstructor(types);
			var actual_parameters = ctor.GetParameters();
			var parameters = new ExprTree.ParameterExpression[actual_parameters.Length];
			var call_params = new ExprTree.Expression[actual_parameters.Length];
			for(int i = 0; i < actual_parameters.Length; ++i){
				parameters[i] = ExprTree.Expression.Parameter(typeof(object), "param" + i);
				call_params[i] = ExprTree.Expression.Convert(parameters[i], actual_parameters[i].ParameterType);
			}

			var call = ExprTree.Expression.New(ctor, call_params);
			return ExprTree.Expression.Lambda(call, parameters);
		}

		public static ExprTree.LambdaExpression MakeNativeMethodCall(Type targetType, string methodName, params Type[] types)
		{
			var inst_param = ExprTree.Expression.Parameter(typeof(object), "inst");
			var method = targetType.GetMethod(methodName, types);
			var actual_parameters = method.GetParameters();
			var param_len = actual_parameters.Length + 1;	//暗黙のthisとなるインスタンスが存在するため、ラムダ式の引数は１つ多くなる
			var call_params = new ExprTree.Expression[actual_parameters.Length];
			var parameters = new ExprTree.ParameterExpression[param_len];

			parameters[0] = inst_param;

			for(int i = 1; i < param_len; ++i){
				parameters[i] = ExprTree.Expression.Parameter(typeof(object), "param" + i);
				call_params[i - 1] = ExprTree.Expression.Convert(parameters[i], actual_parameters[i - 1].ParameterType);
			}

			var call = ExprTree.Expression.Call(ExprTree.Expression.Convert(inst_param, targetType), method, call_params);
			return ExprTree.Expression.Lambda(call, parameters);
		}
	}

	/// <summary>
	/// Helper class for manipulating method's info in Expresso.
	/// </summary>
	/*public class MethodContainer
	{
		private FunctionDeclaration method;
		private object inst;

		/// <summary>
		/// A function instance that points to the method.
		/// </summary>
		public FunctionDeclaration Method{get{return this.method;}}

		/// <summary>
		/// The object on which the method will be called.
		/// </summary>
		public object Inst{get{return this.inst;}}

		public MethodContainer(FunctionDeclaration method, object inst)
		{
			this.method = method;
			this.inst = inst;
		}
	}*/
}

