using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Expresso.Ast;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Helpers
{
	/// <summary>
	/// Expressoの実装用のヘルパー関数郡。
	/// Helper functions for implementing Expresso.
	/// </summary>
	public static class ImplementaionHelpers
	{
		/// <summary>
		/// Determines whether the <paramref name="target"/> is of the specified type.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the <paramref name="target"/> is of the specified type; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='target'>
		/// An ExpressoObject to be tested.
		/// </param>
		/// <param name='type'>
		/// The target type that the object will be tested against.
		/// </param>
		public static bool IsOfType(ExpressoObj target, TYPES type)
		{
			return target.Type == type;
		}

		public static string Replace(this string str, Regex reg, string replacedWith)
		{
			return reg.Replace(str, replacedWith);
		}

		public static string Replace(this string str, Regex reg, MatchEvaluator replacer)
		{
			return reg.Replace(str, replacer);
		}

		public static TYPES GetTypeInExpresso(Type t)
		{
			switch(t.Name){
			case "Int32":
			case "Int64":
				return TYPES.INTEGER;

			case "Double":
				return TYPES.FLOAT;

			case "String":
				return TYPES.STRING;

			default:
				throw new EvalException(string.Format("{0} is not a primitive type in Expresso.", t.FullName));
			}
		}

		public static ExpressoObj GetDefaultValueFor(TYPES type)
		{
			switch(type){
			case TYPES.INTEGER:
				return new ExpressoPrimitive{Value = default(int)};
				
			case TYPES.BOOL:
				return new ExpressoPrimitive{Value = default(bool)};
				
			case TYPES.FLOAT:
				return new ExpressoPrimitive{Value = default(double)};
				
			case TYPES.STRING:
				return new ExpressoPrimitive{Value = default(string)};

			case TYPES.VAR:
				return null;
				
			default:
				throw new EvalException("Unknown object type");
			}
		}

		public static IEnumerable<Parameter> CollectLocalVars(Expression expr)
		{
			if(expr.Type == NodeType.VarDecl)
				return ((VarDeclaration)expr).Variables;
			else
				throw new EvalException("Unreachable code reached!");
		}

		public static IEnumerable<Parameter> CollectLocalVars(Statement stmt)
		{
			if(stmt == null) return Enumerable.Empty<Parameter>();

			IEnumerable<Parameter> result = Enumerable.Empty<Parameter>();
			switch(stmt.Type){
			case NodeType.ExprStatement:
				var expr_stmt = (ExprStatement)stmt;
				result = 
					from p in expr_stmt.Expressions
					where p.Type == NodeType.VarDecl
					select ImplementaionHelpers.CollectLocalVars(p) into t
					from q in t
					select q;
				break;

			case NodeType.SwitchStatement:
				result = ((SwitchStatement)stmt).Cases.SelectMany(x => ImplementaionHelpers.CollectLocalVars(x.Body));
				break;

			case NodeType.IfStatement:
				var if_stmt = (IfStatement)stmt;
				var in_true = ImplementaionHelpers.CollectLocalVars(if_stmt.TrueBlock);
				var in_false = ImplementaionHelpers.CollectLocalVars(if_stmt.FalseBlock);
				result = in_true.Concat(in_false);
				break;

			case NodeType.ForStatement:
				var for_stmt = (ForStatement)stmt;
				result = ImplementaionHelpers.CollectLocalVars(for_stmt.Body);
				break;

			case NodeType.WhileStatement:
				var while_stmt = (WhileStatement)stmt;
				result = ImplementaionHelpers.CollectLocalVars(while_stmt.Body);
				break;

			case NodeType.Block:
				result = ImplementaionHelpers.CollectLocalVars((Block)stmt);
				break;
			}

			return result;
		}

		public static IEnumerable<Parameter> CollectLocalVars(Block root)
		{
			var vars = 
				from p in root.Statements
				where p.Type == NodeType.ExprStatement || p.Type == NodeType.ForStatement || p.Type == NodeType.IfStatement || p.Type == NodeType.SwitchStatement || p.Type == NodeType.WhileStatement
				select ImplementaionHelpers.CollectLocalVars(p) into t
				from q in t
				select q;

			return vars;
		}

		public static void RemoveLast<T>(this List<T> list)
		{
			var len = list.Count;
			if(len <= 0) throw new Exception("Can not delete last element of a list with zero elements!");
			list.RemoveAt(len - 1);
		}
	}

	/// <summary>
	/// ExpressoのSequenceを生成するクラスの実装用のインターフェイス。
	/// </summary>
	public interface SequenceGenerator<C, V>
	{
		V Generate();
		C Take(int count);
	}

	/// <summary>
	/// リストのジェネレーター。Comprehension構文から生成される。
	/// </summary>
	public class ListGenerator : SequenceGenerator<ExpressoList, ExpressoObj>
	{
		public ExpressoObj Generate()
		{
			return null;
		}

		public ExpressoList Take(int count)
		{
			var tmp = new List<ExpressoObj>();
			for(int i = 0; i < count; ++i){

			}
			return null;
		}
	}
}

