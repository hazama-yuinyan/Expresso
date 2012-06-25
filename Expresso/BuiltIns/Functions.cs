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
		public static ExpressoList Take(ExpressoIntegerSequence range, int count)
		{
			return range.Take(count);
		}

		public static ExpressoTuple Zip(params ExpressoObj[] objs)
		{
			var tmp = new List<ExpressoObj>(objs);
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
		public static ExpressoPrimitive Substitute(ExpressoPrimitive str, Dictionary<string, int> orderTable, params ExpressoObj[] vars)
		{
			if(str.Type != TYPES.STRING)
				throw new EvalException("This function takes a string as the first parameter.");

			string tmp = (string)str.Value;
			tmp = tmp.Replace(substitution_refs, m => {
				int result;
				if(!Int32.TryParse(m.Groups[0].Value, out result)){
					var index = orderTable[m.Groups[0].Value];
					return (string)((ExpressoPrimitive)vars[index]).Value;
				}else{
					return (string)((ExpressoPrimitive)vars[result]).Value;
				}
			});

			return new ExpressoPrimitive{Value = tmp, Type = TYPES.STRING};
		}

		/// <summary>
		/// Format the specified str in the way like the printf of C language does.
		/// </summary>
		/// <param name='str'>
		/// The string containing formats.
		/// </param>
		/// <param name='vars'>
		/// Variables.
		/// </param>
		public static ExpressoPrimitive Format(ExpressoPrimitive str, params ExpressoObj[] vars)
		{
			if(str.Type != TYPES.STRING)
				throw new EvalException("This function takes a string as the first parameter.");

			string tmp = (string)str.Value;
			var matches = format_refs.Matches(tmp);
			if(matches.Count < vars.Length)
				throw new EvalException("Too many arguments passed in.");
			else if(matches.Count > vars.Length)
				throw new EvalException("Too few arguments passed in.");

			foreach (var item in matches) {

			}
			return null;
		}
		#endregion
		#region Expressoのシーケンス生成関数郡
		public static ExpressoTuple MakeTuple(List<ExpressoObj> objs)
		{
			return new ExpressoTuple(objs);
		}
		
		public static ExpressoList MakeList(List<ExpressoObj> objs)
		{
			return new ExpressoList{Contents = objs};
		}
		
		public static ExpressoDict MakeDict(List<ExpressoObj> keys, List<ExpressoObj> values, int count)
		{
			var tmp = new Dictionary<ExpressoObj, ExpressoObj>(count);
			for (int i = 0; i < count; ++i) {
				tmp.Add(keys[i], values[i]);
			}
			return new ExpressoDict{Contents = tmp};
		}
		#endregion
	}
}

