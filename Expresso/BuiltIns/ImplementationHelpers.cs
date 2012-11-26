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

namespace Expresso.Helpers
{
	/// <summary>
	/// Expressoの実装用のヘルパー関数郡。
	/// Helper functions for implementing Expresso.
	/// </summary>
	public static class ImplementationHelpers
	{
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

			case "ExpressoTuple":
				return TYPES.TUPLE;

			case "ExpressoIntegerSequence":
				return TYPES.SEQ;

			default:
				if(t.Name.StartsWith("List"))
					return TYPES.LIST;
				else if(t.Name.StartsWith("Dictionary"))
					return TYPES.DICT;
				else
					throw new EvalException(string.Format("{0} is not a primitive type in Expresso.", t.FullName));
			}
		}

		public static TypeAnnotation GetTypeAnnoInExpresso(Type t)
		{
			return new TypeAnnotation(GetTypeInExpresso(t));
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

			case TYPES.CLASS:
			case TYPES.FUNCTION:
			case TYPES.EXPRESSION:
			case TYPES.BYTEARRAY:
			case TYPES.DICT:
			case TYPES.LIST:
			case TYPES.TUPLE:
			case TYPES.VAR:
			case TYPES._INFERENCE:
				return null;
				
			default:
				throw new EvalException("Unknown object type");
			}
		}

		public static Type GetNativeType(TYPES objType)
		{
			switch(objType){
			case TYPES.INTEGER:
				return typeof(int);

			case TYPES.BOOL:
				return typeof(bool);

			case TYPES.FLOAT:
				return typeof(double);

			case TYPES.BIGINT:
				return typeof(BigInteger);

			case TYPES.RATIONAL:
				return typeof(Fraction);

			case TYPES.LIST:
				return typeof(List<object>);

			case TYPES.TUPLE:
				return typeof(ExpressoTuple);

			case TYPES.DICT:
				return typeof(Dictionary<object, object>);

			case TYPES.SEQ:
				return typeof(ExpressoIntegerSequence);

			case TYPES.STRING:
				return typeof(string);

			case TYPES.UNDEF:
				return typeof(void);

			default:
				return null;
			}
		}

		/// <summary>
		/// C#のオブジェクトを必要に応じてExpressoのオブジェクトに変換する。
		/// Wraps C#'s object in an Expresso object when needed.
		/// </summary>
		/*public static object WrapObject(object original, TYPES objType)
		{
			switch(objType){
			case TYPES.DICT:
				return ExpressoFunctions.MakeDict(original as Dictionary<object, object>);

			case TYPES.LIST:
				return ExpressoFunctions.MakeList(original as List<object>);

			case TYPES.SEQ:
				return ExpressoIntegerSequence.Construct((ExpressoIntegerSequence)original);

			case TYPES.TUPLE:
				return ExpressoFunctions.MakeTuple(original as List<object>);

			default:
				return original;
			}
		}*/

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

		/// <summary>
		/// Removes the last element of the list.
		/// </summary>
		/// <param name='list'>
		/// The target List.
		/// </param>
		/// <typeparam name='T'>
		/// The element type of the list.
		/// </typeparam>
		public static void RemoveLast<T>(this List<T> list)
		{
			var len = list.Count;
			if(len <= 0) throw new Exception("Can not delete last element of a list with zero elements!");
			list.RemoveAt(len - 1);
		}

		/// <summary>
		/// Calculates the GCD(Greatest common deivisor).
		/// </summary>
		/// <returns>
		/// The GCD of the two unsigned long values.
		/// </returns>
		/// <param name='first'>
		/// First.
		/// </param>
		/// <param name='second'>
		/// Second.
		/// </param>
		public static BigInteger CalcGCD(BigInteger first, BigInteger second)
		{
			BigInteger r, a = (first > second) ? first : second, b = (first > second) ? second : first, last = b;
			while(true){
				r = a - b;
				if(r == 0) break;
				last = r;
				a = (b > r) ? b : r; b = (b > r) ? r : b;
			}
			
			return last;
		}

		/// <summary>
		/// Calculates the LCM(Least common multiple).
		/// </summary>
		/// <returns>
		/// The LCM of the two unsigned long values.
		/// </returns>
		/// <param name='first'>
		/// First.
		/// </param>
		/// <param name='second'>
		/// Second.
		/// </param>
		public static BigInteger CalcLCM(BigInteger first, BigInteger second)
		{
			BigInteger gcd = CalcGCD(first, second);
			return first * second / gcd;
		}

		public static ExpressoClass.ClassDefinition GetClassInfoFromRef(Expression reference)
		{
			return null;
		}

		public static List<Argument> CreateArgList(params Identifier[] args)
		{
			var result = new List<Argument>();
			foreach(var arg in args)
				result.Add(new Argument{Ident = arg});

			return result;
		}

		/*public static void AddBuiltinObjects()
		{
			ExpressoList.AddDefinition();
			ExpressoDictionary.AddDefinition();
			ExpressoIntegerSequence.AddDefinition();
			ExpressoTuple.AddDefinition();
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

		/// <summary>
		/// IntegerSequenceを使ってコンテナの一部の要素をコピーした新しいコンテナを生成する。
		/// Do the "slice" operation on the container with an IntegerSequence.
		/// </summary>
		public static object Slice(object src, ExpressoIntegerSequence seq)
		{
			object result;
			var enumerator = seq.GetEnumerator();

			if(src is List<object> || src is ExpressoTuple){
				var er = ((IEnumerable<object>)src).GetEnumerator();
				var tmp = new List<object>();
				while(er.MoveNext() && enumerator.MoveNext())
					tmp.Add(er.Current);
					
				if(src is List<object>)
					result = tmp;
				else
					result = ExpressoFunctions.MakeTuple(tmp);
			}else if(src is Dictionary<object, object>){
				var er = ((Dictionary<object, object>)src).GetEnumerator();
				var keys = new List<object>();
				var values = new List<object>();
				while(er.MoveNext() && enumerator.MoveNext()){
					var pair = er.Current as Nullable<KeyValuePair<object, object>>;
					if(pair == null)
						throw new EvalException("Can not evaluate an element to a valid dictionary element.");

					keys.Add(pair.Value.Key);
					values.Add(pair.Value.Value);
				}
				result = ExpressoFunctions.MakeDict(keys, values);
			}else{
				throw new EvalException("This object doesn't support slice operation!");
			}

			return result;
		}

		public static object AccessMember(object target, object subscription)
		{
			if(target is Dictionary<object, object>){
				object value = null;
				((Dictionary<object, object>)target).TryGetValue(subscription, out value);
				return value;
			}else if(subscription is Identifier){
				Identifier ident = (Identifier)subscription;
				Type obj_type = target.GetType();
				string type_name;
				if(target is List<object>)
					type_name = "List";
				else if(target is Dictionary<object, object>)
					type_name = "Dictionary";
				else if(target is ExpressoTuple)
					type_name = "Tuple";
				else if(target is ExpressoIntegerSequence)
					type_name = "IntSeq";
				else
					type_name = obj_type.Name;

				return BuiltinNativeMethods.Instance().LookupMethod(type_name, ident.Name);
			}else if(subscription is int){
				int index = (int)subscription;

				if(target is List<object>)
					return ((List<object>)target)[index];
				else if(target is ExpressoTuple)
					return ((ExpressoTuple)target)[index];
				else
					throw new EvalException("Can not apply the [] operator on that type of object!");
			}else{
				throw new EvalException("Invalid use of accessor!");
			}
		}

		/// <summary>
		/// Assigns an object on a list at a specified index(if "collection" is a list) or assigns an object to a specified key
		/// (if "collection" is a dictionary). An exception would be thrown if the instance is not a valid Expresso's collection.
		/// </summary>
		public static void AssignToCollection(object collection, object target, object value)
		{
			if(target is int){
				int index = (int)target;
				if(collection is List<object>)
					((List<object>)collection)[index] = value;
				else if(collection is ExpressoTuple)
					throw new EvalException("Can not assign a value on a tuple!");
				else
					throw new EvalException("Unknown seqeunce type!");
			}else{
				if(collection is Dictionary<object, object>)
					((Dictionary<object, object>)collection)[target] = value;
				else
					throw new EvalException("Invalid use of the [] operator!");
			}
		}
	}

	/// <summary>
	/// Helper class for manipulating method's info in Expresso.
	/// </summary>
	public class MethodContainer
	{
		private Function method;
		private object inst;

		/// <summary>
		/// A function instance that points to the method.
		/// </summary>
		public Function Method{get{return this.method;}}

		/// <summary>
		/// The object on which the method will be called.
		/// </summary>
		public object Inst{get{return this.inst;}}

		public MethodContainer(Function method, object inst)
		{
			this.method = method;
			this.inst = inst;
		}
	}

	/*internal class ExpressoList
	{
		public static void AddDefinition()
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
				null,
				new NativeMethodNullary<int, List<object>>(
					"length", (List<object> inst) => inst.Count
				),
				new NativeMethodUnary<Void, List<object>, object>(
					"add", new Identifier("elem", TYPES.VAR, 1), (List<object> inst, object elem) => {inst.Add(elem);}
				),
				new NativeMethodUnary<Void, List<object>, IEnumerable<object>>(
					"addRange", new Identifier("elems", TYPES.VAR, 1), (List<object> inst, IEnumerable<object> elems) => {inst.AddRange(elems);}
				),
				new NativeMethodNullary<Void, List<object>>(
					"clear", (List<object> inst) => {inst.Clear();}
				),
				new NativeMethodUnary<bool, List<object>, object>(
					"contains", new Identifier("elem", TYPES.VAR, 1), (List<object> inst, object elem) => inst.Contains(elem)
				),
				new NativeMethodUnary<bool, List<object>, Predicate<object>>(
					"exists", new Identifier("pred", TYPES.FUNCTION, 1), (List<object> inst, Predicate<object> pred) => inst.Exists(pred)
				),
				new NativeMethodUnary<object, List<object>, Predicate<object>>(
					"find", new Identifier("pred", TYPES.FUNCTION, 1), (List<object> inst, Predicate<object> pred) => inst.Find(pred)
				),
				new NativeMethodUnary<List<object>, List<object>, Predicate<object>>(
					"findAll", new Identifier("pred", TYPES.FUNCTION, 1),
					(List<object> inst, Predicate<object> pred) => inst.FindAll(pred)
				),
				new NativeMethodUnary<object, List<object>, Predicate<object>>(
					"findLast", new Identifier("pred", TYPES.FUNCTION, 1), (List<object> inst, Predicate<object> pred) => inst.FindLast(pred)
				),
				new NativeMethodUnary<Void, List<object>, Action<object>>(
					"each", new Identifier("func", TYPES.FUNCTION, 1), (List<object> inst, Action<object> func) => {inst.ForEach(func);}
				),
				new NativeMethodUnary<int, List<object>, object>(
					"indexOf", new Identifier("elem", TYPES.VAR, 1), (List<object> inst, object elem) => inst.IndexOf(elem)
				),
				new NativeMethodBinary<Void, List<object>, object, object>(
					"insert", new Identifier("index", TYPES.INTEGER, 1), new Identifier("elem", TYPES.VAR, 2),
					(List<object> inst, object index, object elem) => {inst.Insert((int)index, elem);}
				),
				new NativeMethodUnary<bool, List<object>, object>(
					"remove", new Identifier("elem", TYPES.VAR, 1), (List<object> inst, object elem) => inst.Remove(elem)
				),
				new NativeMethodUnary<Void, List<object>, int>(
					"removeAt", new Identifier("index", TYPES.INTEGER, 1), (List<object> inst, int index) => {inst.RemoveAt(index);}
				),
				new NativeMethodUnary<int, List<object>, Predicate<object>>(
					"removeAll", new Identifier("pred", TYPES.FUNCTION, 1), (List<object> inst, Predicate<object> pred) => inst.RemoveAll(pred)
				),
				new NativeMethodNullary<Void, List<object>>(
					"reverse", (List<object> inst) => {inst.Reverse();}
				),
				new NativeMethodUnary<Void, List<object>, Comparison<object>>(
					"sort", new Identifier("comp", TYPES.FUNCTION, 1), (List<object> inst, Comparison<object> comp) => {inst.Sort(comp);}
				)
			};

			ExpressoClass.AddClass(definition);
		}

		public static ExpressoClass.ExpressoObj Construct(List<object> inst)
		{
			ExpressoClass.ClassDefinition definition;
			ExpressoClass.Classes.TryGetValue("List", out definition);
			definition.Members[0] = inst;
			return new ExpressoClass.ExpressoObj(definition, TYPES.LIST);
		}
	}

	internal class ExpressoDictionary
	{
		public static void AddDefinition()
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
				null,
				new NativeMethodNullary<int, Dictionary<object, object>>(
					"length", (Dictionary<object, object> inst) => inst.Count
				),
				new NativeMethodBinary<Void, Dictionary<object, object>, object, object>(
					"add", new Identifier("key", TYPES.VAR, 1), new Identifier("val", TYPES.VAR, 2),
					(Dictionary<object, object> inst, object key, object val) => {inst.Add(key, val);}
				),
				new NativeMethodUnary<bool, Dictionary<object, object>, object>(
					"contains", new Identifier("key", TYPES.VAR, 1),
					(Dictionary<object, object> inst, object key) => inst.ContainsKey(key)
				),
				new NativeMethodUnary<object, Dictionary<object, object>, object>(
					"get", new Identifier("key", TYPES.VAR, 1), (Dictionary<object, object> inst, object key) => {
						object value;
						if(inst.TryGetValue(key, out value))
							return value;

						return null;
					}
				),
				new NativeMethodUnary<bool, Dictionary<object, object>, object>(
					"remove", new Identifier("elem", TYPES.VAR, 1),
					(Dictionary<object, object> inst, object key) => inst.Remove(key)
				)
			};

			ExpressoClass.AddClass(definition);
		}

		static public ExpressoClass.ExpressoObj Construct(Dictionary<object, object> inst)
		{
			ExpressoClass.ClassDefinition definition;
			ExpressoClass.Classes.TryGetValue("Dictionary", out definition);
			definition.Members[0] = inst;
			return new ExpressoClass.ExpressoObj(definition, TYPES.DICT);
		}
	}*/
}

