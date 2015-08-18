using System;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using Expresso.Runtime.Builtins;
using Expresso.Runtime;
using Expresso.Runtime.Exceptions;
using Expresso.Runtime.Types;
using Expresso.Runtime.Library;
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
		public static Exception MakeInvalidTypeError(string format, params object[] args)
		{
			return new InvalidTypeException(format, args);
		}

		public static Exception MakeRuntimeError(string format, params object[] args)
		{
			return new RuntimeException(format, args);
		}

		public static Exception MakeImportError(string format, params object[] args)
		{
			return new ImportException(format, args);
		}

		public static Exception MakeReferenceError(string format, params object[] args)
		{
			return new ReferenceException(format, args);
		}

		public static Exception MakeMissingTypeError(string format, params object[] args)
		{
			return new TypeNotFoundException(format, args);
		}

		public static Exception MakeSystemError(string format, params object[] args)
		{
			return new ExpressoSystemException(format, args);
		}

		public static Exception MakeValueError(string format, params object[] args)
		{
			return new InvalidValueException(format, args);
		}

		public static Exception MakeAssertError(string format, params object[] args)
		{
			return new AssertionFailedException(format, args);
		}

		public static Exception MakeIOError(string format, params object[] args)
		{
			return new IOException(string.Format(format, args));
		}

		public static Exception TypeErrorForTypeMismatch(string expectedTypeName, object instance)
		{
            return MakeInvalidTypeError("expected {0}, got {1}", expectedTypeName, instance);
		}
		#endregion

		#region Helpers for making sequences
        public static Dictionary<Key, Value> MakeDict<Key, Value>(List<Key> keys, List<Value> values)
		{
            var tmp = new Dictionary<Key, Value>(keys.Count);
			for(int i = 0; i < keys.Count; ++i)
				tmp.Add(keys[i], values[i]);
			
			return tmp;
		}
		
		public static Dictionary<object, object> MakeDict(Dictionary<object, object> dict)
		{
			if(dict == null)
				throw new ArgumentNullException("dict");
			
			return dict;
		}
		
        public static List<T> MakeVector<T>(List<T> list)
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
        public static IList<T> Slice<T>(IList<T> src, ExpressoIntegerSequence seq)
		{
            List<T> result = new List<T>();
			var enumerator = seq.GetEnumerator();
			
			int index;
			var orig_list = src;
			while(enumerator.MoveNext() && (index = enumerator.Current) < orig_list.Count)
                result.Add(orig_list[index]);
				
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
					throw ExpressoOps.MakeInvalidTypeError("Can not apply the [] operator on that type of object!");
			}else{
				throw ExpressoOps.MakeRuntimeError("Invalid use of accessor!");
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
					throw ExpressoOps.MakeInvalidTypeError("Unknown seqeunce type!");
			}else{
				if(collection is Dictionary<object, object>)
					((Dictionary<object, object>)collection)[target] = value;
				else
					throw ExpressoOps.MakeRuntimeError("Invalid use of the [] operator!");
			}
		}
		
		/// <summary>
        /// Enumerates a dictionary into a tuple.
		/// </summary>
        /// <param name="dict">A dictionary object to enumerate on</param>
		/// <returns>An IEnumerator object</returns>
        public static IEnumerator<Tuple<Key, Value>> Enumerate<Key, Value>(Dictionary<Key, Value> dict)
		{
            if(dict == null)
                throw new ArgumentNullException("dict");

			foreach(var elem in dict)
                yield return new Tuple<Key, Value>(elem.Key, elem.Value);
		}

		internal static string Replace(this string str, Regex regexp, string replacedWith)
		{
			return regexp.Replace(str, replacedWith);
		}
		
		internal static string Replace(this string str, Regex regexp, MatchEvaluator replacer)
		{
			return regexp.Replace(str, replacer);
		}
	}
}

