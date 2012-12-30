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

namespace Expresso.Runtime
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

		public static ObjectTypes GetTypeInExpresso(Type t)
		{
			switch(t.Name){
			case "Int32":
			case "Int64":
				return ObjectTypes.INTEGER;

			case "Double":
				return ObjectTypes.FLOAT;

			case "String":
				return ObjectTypes.STRING;

			case "Boolean":
				return ObjectTypes.BOOL;

			case "Object":
				return ObjectTypes.VAR;

			case "Void":
				return ObjectTypes.UNDEF;

			case "ExpressoTuple":
				return ObjectTypes.TUPLE;

			case "ExpressoIntegerSequence":
				return ObjectTypes.SEQ;

			default:
				if(t.Name.StartsWith("List"))
					return ObjectTypes.LIST;
				else if(t.Name.StartsWith("Dictionary"))
					return ObjectTypes.DICT;
				else
					throw ExpressoOps.InvalidTypeError(string.Format("{0} is not a primitive type in Expresso.", t.FullName));
			}
		}

		public static TypeAnnotation GetTypeAnnoInExpresso(Type t)
		{
			return new TypeAnnotation(GetTypeInExpresso(t));
		}

		public static object GetDefaultValueFor(ObjectTypes type)
		{
			switch(type){
			case ObjectTypes.INTEGER:
				return default(int);
				
			case ObjectTypes.BOOL:
				return default(bool);
				
			case ObjectTypes.FLOAT:
				return default(double);
				
			case ObjectTypes.STRING:
				return default(string);

			case ObjectTypes.BIGINT:
				return new BigInteger();

			case ObjectTypes.RATIONAL:
				return new Fraction();

			case ObjectTypes.INSTANCE:
			case ObjectTypes.FUNCTION:
			case ObjectTypes.EXPRESSION:
			case ObjectTypes.BYTEARRAY:
			case ObjectTypes.DICT:
			case ObjectTypes.LIST:
			case ObjectTypes.TUPLE:
			case ObjectTypes.VAR:
			case ObjectTypes._INFERENCE:
				return null;
				
			default:
				throw ExpressoOps.InvalidTypeError("Unknown object type");
			}
		}

		public static Type GetNativeType(ObjectTypes objType)
		{
			switch(objType){
			case ObjectTypes.INTEGER:
				return typeof(int);

			case ObjectTypes.BOOL:
				return typeof(bool);

			case ObjectTypes.FLOAT:
				return typeof(double);

			case ObjectTypes.BIGINT:
				return typeof(BigInteger);

			case ObjectTypes.RATIONAL:
				return typeof(Fraction);

			case ObjectTypes.LIST:
				return typeof(List<object>);

			case ObjectTypes.TUPLE:
				return typeof(ExpressoTuple);

			case ObjectTypes.DICT:
				return typeof(Dictionary<object, object>);

			case ObjectTypes.SEQ:
				return typeof(ExpressoIntegerSequence);

			case ObjectTypes.STRING:
				return typeof(string);

			case ObjectTypes.UNDEF:
				return typeof(void);

			case ObjectTypes.INSTANCE:
			case ObjectTypes.TYPE_CLASS:
			case ObjectTypes.TYPE_MODULE:
				return typeof(ExpressoObj);

			default:
				return null;
			}
		}

		public static IEnumerable<Identifier> CollectLocalVars(Expression expr)
		{
			if(expr.Type == NodeType.VarDecl)
				return ((VarDeclaration)expr).Variables;
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
		/// The GCD of the two BigIntegers.
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
		/// The LCM of the two BigIntegers.
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

		public static Argument MakeArg(Identifier ident, Expression option = null)
		{
			return new Argument{Ident = ident, Option = option};
		}

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

			if(src is List<object>){
				int index;
				var tmp = new List<object>();
				var orig_list = (List<object>)src;
				while(enumerator.MoveNext() && (index = (int)enumerator.Current) < orig_list.Count)
					tmp.Add(orig_list[index]);

				result = tmp;
			}else if(src is ExpressoTuple){
				int index;
				var orig_tuple = (ExpressoTuple)src;
				var tmp = new List<object>();
				while(enumerator.MoveNext() && (index = (int)enumerator.Current) < orig_tuple.Count)
					tmp.Add(orig_tuple[index]);
					
				result = ExpressoOps.MakeTuple(tmp);
			}else{
				throw ExpressoOps.InvalidTypeError("This object doesn't support slice operation!");
			}

			return result;
		}

		public static object AccessMember(Identifier ident, object target, object subscription)
		{
			if(target is Dictionary<object, object>){
				object value = null;
				((Dictionary<object, object>)target).TryGetValue(subscription, out value);
				return value;
			}else if(subscription is Identifier){
				Identifier subscript = (Identifier)subscription;
				string type_name;
				if(ident.ParamType.TypeName != null)
					type_name = ident.ParamType.TypeName;
				else if(target is List<object>)
					type_name = "List";
				else if(target is ExpressoTuple)
					type_name = "Tuple";
				else if(target is Dictionary<object, object>)
					type_name = "Dictionary";
				else if(target is FileObject)
					type_name = "File";
				else
					type_name = target.GetType().FullName;

				return BuiltinNativeMethods.Instance().LookupMethod(type_name, subscript.Name);
			}else if(subscription is int){
				int index = (int)subscription;

				if(target is List<object>)
					return ((List<object>)target)[index];
				else if(target is ExpressoTuple)
					return ((ExpressoTuple)target)[index];
				else
					throw ExpressoOps.InvalidTypeError("Can not apply the [] operator on that type of object!");
			}else{
				throw ExpressoOps.RuntimeError("Invalid use of accessor!");
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
					throw new InvalidOperationException("Can not assign a value on a tuple!");
				else
					throw ExpressoOps.InvalidTypeError("Unknown seqeunce type!");
			}else{
				if(collection is Dictionary<object, object>)
					((Dictionary<object, object>)collection)[target] = value;
				else
					throw ExpressoOps.RuntimeError("Invalid use of the [] operator!");
			}
		}

		public static IEnumerator<object> Enumerate(object enumerable)
		{
			if(enumerable is IEnumerable<object>){
				var enumerator = ((IEnumerable<object>)enumerable).GetEnumerator();
				while(enumerator.MoveNext())
					yield return enumerator.Current;
			}else if(enumerable is Dictionary<object, object>){
				var dict = (Dictionary<object, object>)enumerable;
				foreach(var elem in dict)
					yield return ExpressoOps.MakeTuple(new object[]{elem.Key, elem.Value});
			}else
				throw ExpressoOps.RuntimeError("Unknown object type!");
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
}

