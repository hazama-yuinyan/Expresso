using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Expresso.Ast;
using Expresso.Builtins;
using Expresso.Builtins.Library;
using Expresso.Interpreter;
using Expresso.Helpers;


namespace Expresso.Builtins
{
	using Helpers = Expresso.Helpers.ImplementationHelpers;

	/// <summary>
	/// Expressoの組み込み関数郡。
	/// The built-in functions for Expresso.
	/// </summary>
	public static class ExpressoFunctions
	{
		private static Regex substitution_refs = new Regex(@"\$\{(\d+|[a-zA-Z_][a-zA-Z_0-9]+)\}");

		private static Regex format_refs = new Regex(@"%([0-9.]*)([boxXsdfcueEgG])");

		#region Expressoの組み込み関数郡
		#region 数学関数郡
		public static object Abs(object val)
		{
			if(val is int)
				return Math.Abs((int)val);
			else if(val is double)
				return Math.Abs((double)val);
			else if(val is BigInteger)
				return BigInteger.Abs((BigInteger)val);
			else
				return null;
		}

		public static object Sqrt(object val)
		{
			double tmp = 1.0;
			if(val is int)
				tmp = (double)((int)val);
			else if(val is double)
				tmp = (double)val;

			return Math.Sqrt(tmp);
		}
		#endregion

		public static object ToInt(object val)
		{
			if(val is double)
				return (int)((double)val);
			else if(val is int)
				return val;
			else
				return null;
		}
		/// <summary>
		/// Similar to the same named function in Haskell, it takes a certain number of elements
		/// from a sequence.
		/// </summary>
		/// <param name='range'>
		/// Range.
		/// </param>
		/// <param name='count'>
		/// Count.
		/// </param>
		/*public static List<object> Take(ExpressoIntegerSequence range, int count)
		{
			return range.Take(count);
		}*/

		public static ExpressoTuple Zip(params object[] objs)
		{
			var tmp = new List<object>(objs);
			return new ExpressoTuple(tmp);
		}

		/// <summary>
		/// Replace substitutions in a string with the corresponding values.
		/// </summary>
		/// <param name='str'>
		/// The string containing substitutions.
		/// </param>
		/// <param name='vars'>
		/// The objects to be substituted for.
		/// </param>
		public static string Substitute(string str, Dictionary<string, int> orderTable, params object[] vars)
		{
			if(str == null)
				throw new EvalException("This function takes a string as the first parameter.");

			string tmp = str;
			tmp = tmp.Replace(substitution_refs, m => {
				int result;
				if(!Int32.TryParse(m.Groups[0].Value, out result)){
					var index = orderTable[m.Groups[0].Value];
					return (string)vars[index];
				}else{
					return (string)vars[result];
				}
			});

			return tmp;
		}

		/// <summary>
		/// Format the specified str in the way like the printf of the C language does.
		/// </summary>
		/// <param name='str'>
		/// The string containing formats.
		/// </param>
		/// <param name='vars'>
		/// Variables.
		/// </param>
		public static string Format(string str, params object[] vars)
		{
			if(str == null)
				throw new ArgumentNullException("str");

			string tmp = str;
			int i = 0;
			tmp = tmp.Replace(format_refs, m => {
				if(m.Groups[2].Value == "b"){
					if(!(vars[i] is int))
						throw new EvalException("Can not format objects in binary except an integer!");

					var sb = new StringBuilder();
				
					uint target = (uint)vars[i];
					int max_digits = (m.Groups[1].Value == "") ? -1 : Convert.ToInt32(m.Groups[1].Value);
					for(int bit_pos = 0; target > 0 && max_digits < bit_pos; ++bit_pos){
						var bit = target & (0x01 << bit_pos);
						sb.Append(bit);
					}
					++i;
					return new string(sb.ToString().Reverse().ToArray());
				}else{
					return "{" + i++ + ":" + m.Groups[1].Value + "}";
				}
			});

			return string.Format(tmp, vars);
		}

		/// <summary>
		/// Iterates some sequence and returns a tuple containing an index and the corresponding element.
		/// </summary>
		public static IEnumerable<ExpressoTuple> Each(IEnumerable<object> src)
		{
			var er = src.GetEnumerator();
			if(!er.MoveNext())
				throw new InvalidOperationException();

			for(int i = 0; er.MoveNext(); ++i)
				yield return new ExpressoTuple(new []{i, er.Current});
		}
		#endregion
		#region Expressoのシーケンス生成関数郡
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
	}

	public sealed class BuiltinNativeMethods
	{
		private static BuiltinNativeMethods inst = null;

		private Dictionary<string, Dictionary<string, NativeFunction>> native_methods;

		private BuiltinNativeMethods()
		{
			var list = new Dictionary<string, NativeFunction>{
				{"add", new NativeFunctionNAry("add", Helpers.MakeNativeMethodCall(typeof(List<object>), "Add", typeof(object)))},
				{"clear", new NativeFunctionNAry("clear", Helpers.MakeNativeMethodCall(typeof(List<object>), "Clear"))},
				{"contains", new NativeFunctionNAry("contains", Helpers.MakeNativeMethodCall(typeof(List<object>), "Contains", typeof(object)))}
			};

			var tuple = new Dictionary<string, NativeFunction>{
				{"empty", new NativeFunctionNAry("empty", Helpers.MakeNativeMethodCall(typeof(ExpressoTuple), "Empty"))},
				{"contains", new NativeFunctionNAry("contains", Helpers.MakeNativeMethodCall(typeof(ExpressoTuple), "Contains", typeof(object)))}
			};

			var dict = new Dictionary<string, NativeFunction>{
				{"add", new NativeFunctionNAry("add", Helpers.MakeNativeMethodCall(typeof(Dictionary<object, object>), "Add", typeof(object), typeof(object)))},
				{"contains", new NativeFunctionNAry("contains", Helpers.MakeNativeMethodCall(typeof(Dictionary<object, object>), "ContainsKey", typeof(object)))},
				{"remove", new NativeFunctionNAry("remove", Helpers.MakeNativeMethodCall(typeof(Dictionary<object, object>), "Remove", typeof(object)))},
			};

			var file_obj = new Dictionary<string, NativeFunction>{
				{"constructor", new NativeLambdaTernary("constructor", Helpers.MakeArg(new Identifier("path", new TypeAnnotation(TYPES.STRING))),
					                                        Helpers.MakeArg(new Identifier("option", new TypeAnnotation(TYPES.STRING))),
					                                        Helpers.MakeArg(new Identifier("encoding", new TypeAnnotation(TYPES.STRING)), new Constant{ValType = TYPES.STRING, Value = "UTF-8"}),
					                                        (object path, object option, object encoding) => FileObject.OpenFile((string)path, (string)option, (string)encoding))},
				{"read", new NativeFunctionNAry("read", Helpers.MakeNativeMethodCall(typeof(FileObject), "Read"))},
				{"readLine", new NativeFunctionNAry("readLine", Helpers.MakeNativeMethodCall(typeof(FileObject), "ReadLine"))},
				{"readAll", new NativeFunctionNAry("readAll", Helpers.MakeNativeMethodCall(typeof(FileObject), "ReadAll"))},
				{"write", new NativeFunctionNAry("write", Helpers.MakeNativeMethodCall(typeof(FileObject), "Write", typeof(object)))},
				{"openFile", new NativeLambdaTernary("openFile", Helpers.MakeArg(new Identifier("path", new TypeAnnotation(TYPES.STRING))),
					                                     Helpers.MakeArg(new Identifier("option", new TypeAnnotation(TYPES.STRING))),
					                                     Helpers.MakeArg(new Identifier("encoding", new TypeAnnotation(TYPES.STRING)), new Constant{ValType = TYPES.STRING, Value = "UTF-8"}),
					                                     (object path, object option, object encoding) => FileObject.OpenFile((string)path, (string)option, (string)encoding))}
			};

			native_methods = new Dictionary<string, Dictionary<string, NativeFunction>>{
				{"List", list},
				{"Tuple", tuple},
				{"Dictionary", dict},
				{"File", file_obj}
			};
		}

		public static BuiltinNativeMethods Instance()
		{
			if(inst == null)
				inst = new BuiltinNativeMethods();

			return inst;
		}

		public NativeFunction LookupMethod(string typeName, string methodName)
		{
			Dictionary<string, NativeFunction> type_dict;
			if(!native_methods.TryGetValue(typeName, out type_dict))
				throw new EvalException(typeName + " is not a native class name!");

			NativeFunction method;
			if(!type_dict.TryGetValue(methodName, out method))
				throw new EvalException(typeName + " doesn't have the method \"" + methodName + "\".");

			return method;
		}
	}
}

