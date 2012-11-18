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
	public static class ImplementationHelpers
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
		public static bool IsOfType(TYPES target, TYPES type)
		{
			return target == type;
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

			case "Boolean":
				return TYPES.BOOL;

			case "Object":
				return TYPES.VAR;

			case "Void":
				return TYPES.UNDEF;

			default:
				if(t.Name.StartsWith("List"))
					return TYPES.LIST;
				else
					throw new EvalException(string.Format("{0} is not a primitive type in Expresso.", t.FullName));
			}
		}

		public static object GetDefaultValueFor(TYPES type)
		{
			switch(type){
			case TYPES.INTEGER:
				return default(int);
				
			case TYPES.BOOL:
				return default(bool);
				
			case TYPES.FLOAT:
				return default(double);
				
			case TYPES.STRING:
				return default(string);

			case TYPES.VAR:
				return null;
				
			default:
				throw new EvalException("Unknown object type");
			}
		}

		public static IEnumerable<Identifier> CollectLocalVars(Expression expr)
		{
			if(expr.Type == NodeType.VarDecl)
				return ((VarDeclaration)expr).Variables;
			else if(expr.Type == NodeType.Identifier)
				return new Identifier[]{(Identifier)expr};
			else
				throw new EvalException("Unreachable code reached!");
		}

		public static IEnumerable<Identifier> CollectLocalVars(Statement stmt)
		{
			if(stmt == null) return Enumerable.Empty<Identifier>();

			IEnumerable<Identifier> result = Enumerable.Empty<Identifier>();
			switch(stmt.Type){
			case NodeType.ExprStatement:
				var expr_stmt = (ExprStatement)stmt;
				result = 
					from p in expr_stmt.Expressions
					where p.Type == NodeType.VarDecl
					select ImplementationHelpers.CollectLocalVars(p) into t
					from q in t
					select q;
				break;

			case NodeType.SwitchStatement:
				result = ((SwitchStatement)stmt).Cases.SelectMany(x => ImplementationHelpers.CollectLocalVars(x.Body));
				break;

			case NodeType.IfStatement:
				var if_stmt = (IfStatement)stmt;
				var in_true = ImplementationHelpers.CollectLocalVars(if_stmt.TrueBlock);
				var in_false = ImplementationHelpers.CollectLocalVars(if_stmt.FalseBlock);
				result = in_true.Concat(in_false);
				break;

			case NodeType.ForStatement:
				var for_stmt = (ForStatement)stmt;
				IEnumerable<Identifier> in_cond = Enumerable.Empty<Identifier>();
				if(for_stmt.HasLet){
					in_cond =
						from p in for_stmt.LValues
						select ImplementationHelpers.CollectLocalVars(p) into t
						from q in t
						select q;
				}

				var in_body = ImplementationHelpers.CollectLocalVars(for_stmt.Body);
				result = in_cond.Concat(in_body);
				break;

			case NodeType.WhileStatement:
				var while_stmt = (WhileStatement)stmt;
				result = ImplementationHelpers.CollectLocalVars(while_stmt.Body);
				break;

			case NodeType.Block:
				result = ImplementationHelpers.CollectLocalVars((Block)stmt);
				break;
			}

			return result;
		}

		public static IEnumerable<Identifier> CollectLocalVars(Block root)
		{
			var vars = 
				from p in root.Statements
				where p.Type == NodeType.ExprStatement || p.Type == NodeType.ForStatement || p.Type == NodeType.IfStatement || p.Type == NodeType.SwitchStatement || p.Type == NodeType.WhileStatement
				select ImplementationHelpers.CollectLocalVars(p) into t
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

		public static ulong CalcGDC(ulong first, ulong second)
		{
			ulong r, a = (first > second) ? first : second, b = (first > second) ? second : first, last = b;
			while(true){
				r = a - b;
				if(r == 0) break;
				last = r;
				a = (b > r) ? b : r; b = (b > r) ? r : b;
			}
			
			return last;
		}
		
		public static ulong CalcLCM(ulong first, ulong second)
		{
			ulong gdc = CalcGDC(first, second);
			return first * second / gdc;
		}
	}

	/// <summary>
	/// ExpressoのSequenceを生成するクラスの実装用のインターフェイス。
	/// </summary>
	public interface SequenceGenerator<Container, Value>
	{
		Value Generate();
		Container Take(int count);
		Container TakeAll();
	}

	/// <summary>
	/// リストのジェネレーター。Comprehension構文から生成される。
	/// </summary>
	public class ListGenerator : SequenceGenerator<List<object>, object>
	{
		private IEnumerable<object> source;

		public ListGenerator(IEnumerable<object> source)
		{
			this.source = source;
		}

		public object Generate()
		{
			return null;
		}

		public List<object> Take(int count)
		{
			var tmp = new List<object>(count);
			int i = 0;
			foreach(var elem in source){
				if(i >= count) break;

				tmp.Add(elem);
				++i;
			}

			return tmp;
		}

		public List<object> TakeAll()
		{
			var tmp = new List<object>(source);
			return tmp;
		}
	}

	internal class ExpressoList
	{
		public static ExpressoClass.ExpressoObj Construct(List<object> inst)
		{
			var privates = new Dictionary<string, int>();
			privates.Add("content", 0);

			var publics = new Dictionary<string, int>();
			publics.Add("length", 1);
			publics.Add("add", 2);
			publics.Add("addRange", 3);
			publics.Add("clear", 4);
			publics.Add("contains", 5);
			publics.Add("exists", 6);
			publics.Add("find", 7);
			publics.Add("findAll", 8);
			publics.Add("findLast", 9);
			publics.Add("each", 10);
			publics.Add("indexOf", 11);
			publics.Add("insert", 12);
			publics.Add("remove", 13);
			publics.Add("removeAt", 14);
			publics.Add("removeAll", 15);
			publics.Add("reverse", 16);
			publics.Add("sort", 17);

			var definition = new ExpressoClass.ClassDefinition("List", privates, publics);
			definition.Members = new object[]{
				inst,
				new NativeFunctionNullary<int>(
					"length", () => inst.Count
				),
				new NativeFunctionUnary<Void, object>(
					"add", new Argument{Ident = new Identifier("elem", TYPES.VAR, 0)},
					new Func<object, Void>(inst.Add)
				),
				new NativeFunctionUnary<Void, IEnumerable<object>>(
					"addRange", new Argument{Ident = new Identifier("elems", TYPES.VAR, 0)},
					new Func<IEnumerable<object>, Void>(inst.AddRange)
				),
				new NativeFunctionNullary<Void>(
					"clear", new Func<Void>(inst.Clear)
				),
				new NativeFunctionUnary<bool, object>(
					"contains", new Argument{Ident = new Identifier("elem", TYPES.VAR, 0)},
					new Func<object, bool>(inst.Contains)
				),
				new NativeFunctionUnary<bool, Predicate<object>>(
					"exists", new Argument{Ident = new Identifier("pred", TYPES.FUNCTION, 0)},
					new Func<Predicate<object>, bool>(inst.Exists)
				),
				new NativeFunctionUnary<object, Predicate<object>>(
					"find", new Argument{Ident = new Identifier("pred", TYPES.FUNCTION, 0)},
					new Func<Predicate<object>, object>(inst.Find)
				),
				new NativeFunctionUnary<List<object>, Predicate<object>>(
					"findAll", new Argument{Ident = new Identifier("pred", TYPES.FUNCTION, 0)},
					new Func<Predicate<object>, List<object>>(inst.FindAll)
				),
				new NativeFunctionUnary<object, Predicate<object>>(
					"findLast", new Argument{Ident = new Identifier("pred", TYPES.FUNCTION, 0)},
					new Func<Predicate<object>, object>(inst.FindLast)
				),
				new NativeFunctionUnary<Void, Action<object>>(
					"each", new Argument{Ident = new Identifier("func", TYPES.FUNCTION, 0)},
					new Func<Action<object>, Void>(inst.ForEach)
				),
				new NativeFunctionUnary<int, object>(
					"indexOf", new Argument{Ident = new Identifier("elem", TYPES.VAR, 0)},
					new Func<object, int>(inst.IndexOf)
				),
				new NativeFunctionBinary<Void, int, object>(
					"insert", new Argument{Ident = new Identifier("index", TYPES.INTEGER, 0)},
					new Argument{Ident = new Identifier("elem", TYPES.VAR, 1)},
					new Func<int, object, Void>(inst.Insert)
				),
				new NativeFunctionUnary<bool, object>(
					"remove", new Argument{Ident = new Identifier("elem", TYPES.VAR, 0)},
					new Func<object, bool>(inst.Remove)
				),
				new NativeFunctionUnary<Void, int>(
					"removeAt", new Argument{Ident = new Identifier("index", TYPES.INTEGER, 0)},
					new Func<int, Void>(inst.RemoveAt)
				),
				new NativeFunctionUnary<int, Predicate<object>>(
					"removeAll", new Argument{Ident = new Identifier("pred", TYPES.FUNCTION, 0)},
					new Func<Predicate<object>, int>(inst.RemoveAll)
				),
				new NativeFunctionNullary<Void>(
					"reverse", new Func<Void>(inst.Reverse)
				),
				new NativeFunctionUnary<Void, Comparison<object>>(
					"sort", new Argument{Ident = new Identifier("comp", TYPES.FUNCTION, 0)},
					new Func<Comparison<object>, Void>(inst.Sort)
				)
			};

			return new ExpressoClass.ExpressoObj(definition, TYPES.LIST);
		}
	}

	internal class ExpressoDictionary
	{
		public static ExpressoClass.ExpressoObj Construct(Dictionary<object, object> inst)
		{
			var privates = new Dictionary<string, int>();
			privates.Add("content", 0);

			var publics = new Dictionary<string, int>();
			publics.Add("length", 1);
			publics.Add("add", 2);
			publics.Add("contains", 3);
			publics.Add("get", 4);
			publics.Add("remove", 5);

			var definition = new ExpressoClass.ClassDefinition("Dictionary", privates, publics);
			definition.Members = new object[]{
				inst,
				new NativeFunctionNullary<int>(
					"length", () => inst.Count
				),
				new NativeFunctionBinary<Void, object, object>(
					"add", new Argument{Ident = new Identifier("key", TYPES.VAR, 0)},
					new Argument{Ident = new Identifier("val", TYPES.VAR, 1)},
					new Func<object, object, Void>(inst.Add)
				),
				new NativeFunctionUnary<bool, object>(
					"contains", new Argument{Ident = new Identifier("key", TYPES.VAR, 0)},
					new Func<object, bool>(inst.ContainsKey)
				),
				new NativeFunctionUnary<object, object>(
					"get", new Argument{Ident = new Identifier("key", TYPES.VAR, 0)}, (key) => {
						object value;
						if(inst.TryGetValue(key, out value))
							return value;

						return null;
					}
				),
				new NativeFunctionUnary<bool, object>(
					"remove", new Argument{Ident = new Identifier("elem", TYPES.VAR, 0)},
					new Func<object, bool>(inst.Remove)
				)
			};

			return new ExpressoClass.ExpressoObj(definition, TYPES.DICT);
		}
	}
}

