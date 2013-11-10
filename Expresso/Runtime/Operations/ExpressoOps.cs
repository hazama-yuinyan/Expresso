using System;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using Expresso.Builtins;
using Expresso.Runtime;
using Expresso.Runtime.Exceptions;
using Expresso.Runtime.Types;
using Expresso.Ast;
using Expresso.Builtins.Library;
using Expresso.Compiler.Meta;

namespace Expresso.Runtime.Operations
{
	/// <summary>
	/// Expresso固有の処理。
	/// Contains functions that are called directly from
	/// generated code to perform low-level runtime functionality.
	/// </summary>
	public static class ExpressoOps
	{
		#region Helpers for throwing Expresso's various exceptions
		public static Exception InvalidTypeError(string format, params object[] args)
		{
			return new InvalidTypeException(format, args);
		}

		public static Exception RuntimeError(string format, params object[] args)
		{
			return new RuntimeException(format, args);
		}

		public static Exception ImportError(string format, params object[] args)
		{
			return new ImportException(format, args);
		}

		public static Exception ReferenceError(string format, params object[] args)
		{
			return new ReferenceException(format, args);
		}

		public static Exception MissingTypeError(string format, params object[] args)
		{
			return new TypeNotFoundException(format, args);
		}

		public static Exception SystemError(string format, params object[] args)
		{
			return new ExpressoSystemException(format, args);
		}

		public static Exception ValueError(string format, params object[] args)
		{
			return new InvalidValueException(format, args);
		}

		public static Exception AssertError(string format, params object[] args)
		{
			return new AssertionFailedException(format, args);
		}

		public static Exception IOError(string format, params object[] args)
		{
			return new IOException(string.Format(format, args));
		}

		public static Exception TypeErrorForTypeMismatch(string expectedTypeName, object instance)
		{
			return InvalidTypeError("expected {0}, got {1}", expectedTypeName, ExpressoOps.GetExpressoTypeName(instance));
		}
		#endregion

		#region Helpers for making sequences
		public static ExpressoTuple MakeTuple(List<object> objs)
		{
			if(objs == null)
				throw new ArgumentNullException("objs");
			
			return new ExpressoTuple(objs);
		}
		
		public static ExpressoTuple MakeTuple(object[] objs)
		{
			if(objs == null)
				throw new ArgumentNullException("objs");
			
			return new ExpressoTuple(objs);
		}
		
		public static Dictionary<object, object> MakeDict(List<object> keys, List<object> values)
		{
			var tmp = new Dictionary<object, object>(keys.Count);
			for (int i = 0; i < keys.Count; ++i)
				tmp.Add(keys[i], values[i]);
			
			return tmp;
		}
		
		public static Dictionary<object, object> MakeDict(Dictionary<object, object> dict)
		{
			if(dict == null)
				throw new ArgumentNullException("dict");
			
			return dict;
		}
		
		public static List<object> MakeList(List<object> list)
		{
			if(list == null)
				throw new ArgumentNullException("list");
			
			return list;
		}
		#endregion

		/// <summary>
		/// Helper method for comparing native arrays.
		/// </summary>
		public static int CompareArrays(object[] data0, int size0, object[] data1, int size1, IComparer comparer)
		{
			int size = Math.Min(size0, size1);
			for(int i = 0; i < size; ++i){
				int c = comparer.Compare(data0[i], data1[i]);
				if(c != 0) return c;
			}
			if(size0 == size1) return 0;
			return size0 > size1 ? +1 : -1;
		}

		/// <summary>
		/// The implementation of the print statement in Expresso.
		/// </summary>
		/// <param name="values"></param>
		/// <param name="hasTrailing">Flag indicating whether the string to be displayed has a trailing comma.</param>
		public static void DoPrint(List<object> values, bool hasTrailing)
		{
			var first = values[0];
			var sb = new StringBuilder();
			
			if(first is string){	//When the first argument is string, it means that the string is the format string.
				sb.Append(first);
				values.RemoveAt(0);
				for(int i = 1; i < values.Count; ++i){
					sb.Append("{" + (i - 1) + "}");
					if(i + 1 != values.Count)
						sb.Append(",");
				}
				
				var text = string.Format(sb.ToString(), values.ToArray());
				if(!hasTrailing)
					Console.WriteLine(text);
				else
					Console.Write(text);
			}else{
				sb.Append(first);
				bool print_comma = true;
				for(int i = 1; i < values.Count; ++i){
					if(print_comma)
						sb.Append(",");
					
					sb.Append(values[i]);
					print_comma = (values[i] is string) ? false : true;
				}
				
				var text = sb.ToString();
				if(!hasTrailing)
					Console.WriteLine(text);
				else
					Console.Write(text + ",");
			}
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
		
		/// <summary>
		/// Helper method for accessing a member on an Expresso's object.
		/// </summary>
		/// <param name="target">The Expresso's object</param>
		/// <param name="subscription">The expression that determines which member is referenced</param>
		/// <returns></returns>
		public static object AccessMember(object target, object subscription)
		{
			if(target is Dictionary<object, object>){
				object value = null;
				((Dictionary<object, object>)target).TryGetValue(subscription, out value);
				return value;
			}else if(subscription is Identifier){
				Identifier subscript = (Identifier)subscription;
				var exs_type = DynamicHelpers.GetExpressoTypeFromType(target.GetType());
				return exs_type.Def.GetMember(subscript.Name, false);
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
		
		/// <summary>
		/// Enumerates an Expresso's collection object.
		/// </summary>
		/// <param name="enumerable">The object to enumerate on</param>
		/// <returns>An IEnumerator object</returns>
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
		    }else{
				throw ExpressoOps.RuntimeError("Unknown object type!");
		    }
		}

		#region Operations on types
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

			case "ExpressoModule":
				return ObjectTypes.TYPE_MODULE;
				
			default:
				if(t.Name.StartsWith("List"))
					return ObjectTypes.LIST;
				else if(t.Name.StartsWith("Dictionary"))
					return ObjectTypes.DICT;
				else
					throw ExpressoOps.InvalidTypeError(string.Format("{0} is not a primitive type in Expresso.", t.FullName));
			}
		}
		
		public static TypeAnnotation GetTypeAnnotInExpresso(Type t)
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
				return typeof(List<>);
				
			case ObjectTypes.TUPLE:
				return typeof(ExpressoTuple);
				
			case ObjectTypes.DICT:
				return typeof(Dictionary<,>);
				
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

		public static string GetExpressoTypeName(object obj)
		{
			Type type = obj.GetType();
			return GetTypeInExpresso(type).ToString();
		}
		#endregion

		public static Stack<object> MakeEvaluationStackFromContext(CodeContext context)
		{
			return context.EvaluationStack;
		}

		internal static string Replace(this string str, Regex reg, string replacedWith)
		{
			return reg.Replace(str, replacedWith);
		}
		
		internal static string Replace(this string str, Regex reg, MatchEvaluator replacer)
		{
			return reg.Replace(str, replacer);
		}

		public static ExpressoAst ParseAndBind(string srcFileName)
		{
			var parser = new Parser(new Scanner(srcFileName));
			parser.ParsingFileName = srcFileName;
			parser.Parse();
			ExpressoNameBinder.BindAst(parser.TopmostAst, parser);
			return parser.TopmostAst;
		}
	}
}

