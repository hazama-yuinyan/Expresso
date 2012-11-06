using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Expresso.BuiltIns;
using Expresso.Interpreter;
using Expresso.Helpers;

namespace Expresso.BuiltIns
{
	/// <summary>
	/// Expressoの組み込み関数郡。
	/// The built-in functions for Expresso.
	/// </summary>
	public static class ExpressoFunctions
	{
		private static Regex substitution_refs = new Regex(@"\$\{(\d+|[a-zA-Z_][a-zA-Z_0-9]+)\}");

		private static Regex format_refs = new Regex(@"%[0-9.]*[sdfc]");

		#region Expressoの組み込み関数郡
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
		public static List<object> Take(ExpressoIntegerSequence range, int count)
		{
			return range.Take(count);
		}

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
				throw new EvalException("This function takes a string as the first parameter.");

			string tmp = str;
			var matches = format_refs.Matches(tmp);
			if(matches.Count < vars.Length)
				throw new EvalException("Too many arguments passed in.");
			else if(matches.Count > vars.Length)
				throw new EvalException("Too few arguments passed in.");

			foreach (var item in matches) {

			}
			return null;
		}

		public static int CastToInt(double val)
		{
			return (int)val;
		}
		#endregion
		#region Expressoのシーケンス生成関数郡
		public static ExpressoTuple MakeTuple(List<object> objs)
		{
			return new ExpressoTuple(objs);
		}
		
		public static Dictionary<object, object> MakeDict(List<object> keys, List<object> values, int count)
		{
			var tmp = new Dictionary<object, object>(count);
			for (int i = 0; i < count; ++i) {
				tmp.Add(keys[i], values[i]);
			}
			return tmp;
		}

		private static IEnumerable<object> SliceImpl(IEnumerable<object> src, ExpressoIntegerSequence seq)
		{
			var enumerator = seq.GetEnumerator();
			var er = src.GetEnumerator();
			if(!enumerator.MoveNext() || !er.MoveNext())
				throw new InvalidOperationException();

			do{
				yield return er.Current;
			}while(enumerator.MoveNext() && er.MoveNext());
		}

		public static object[] Slice(this object[] src, ExpressoIntegerSequence seq)
		{
			var tmp = new List<object>();
			foreach(var obj in ExpressoFunctions.SliceImpl(src, seq)){
				tmp.Add(obj);
			}
			return tmp.ToArray();
		}

		public static List<object> Slice(this List<object> src, ExpressoIntegerSequence seq)
		{
			var result = new List<object>();
			foreach(var obj in ExpressoFunctions.SliceImpl(src, seq)){
				result.Add(obj);
			}
			return result;
		}

		/*public static Dictionary<object, object> Slice(this Dictionary<object, object> src, ExpressoIntegerSequence seq)
		{
			var result = new Dictionary<object, object>();
			foreach(var obj in ExpressoFunctions.SliceImpl(src, seq)){
				var pair = obj as KeyValuePair<object, object>;
				if(pair == null)
					throw new EvalException("Can not evaluate an element to a valid dictionary element.");

				result.Add(pair.Key, pair.Value);
			}
			return result;
		}*/
		#endregion
	}
}

