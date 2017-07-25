using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

using Expresso.Runtime.Library;
//using Expresso.Interpreter;
using Expresso.Runtime.Exceptions;


namespace Expresso.Runtime.Builtins
{
	/// <summary>
	/// Expressoの組み込み関数郡。
	/// The built-in functions for Expresso.
	/// </summary>
    /// <remarks>
    /// Note that this class only contains those that should be re-implemented.
    /// Functions and methods that are just to be renamed are done so through
    /// <see cref="Expresso.Runtime.TypeMapper"/>
    /// </remarks>
	public static class ExpressoFunctions
	{
        static Regex FormatRefs = 
            new Regex(@"\{(?<arg>\d*|[a-zA-Z_])(:((?<fill>[a-zA-Z_0-9 ']?)(?<align>[<>^]?))?(?<sign>[+\-])?(?<hash>[#])?(?<head>[0])?(?<width>[0-9]*)(?<precision>\.[0-9]+)?(?<type>[oxXpbeE\?]?))?\}");

        static Regex formatRefs = new Regex(@"%([0-9.]*)([boxXsdfcueEgG])");

		#region Expressoの組み込み関数郡

        /// <summary>
        /// Given two iterators, it returns a tuple 
        /// </summary>
        /// <param name="iter">Iter.</param>
        /// <param name="iter2">Iter2.</param>
        /// <typeparam name="T1">The 1st type parameter.</typeparam>
        /// <typeparam name="T2">The 2nd type parameter.</typeparam>
        [ExpressoFunction("zip")]
        public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2>(IEnumerable<T1> iter, IEnumerable<T2> iter2)
		{
            foreach(var item in iter.Zip(iter2, (a, b) => new Tuple<T1, T2>(a, b)))
                yield return item;
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
        /*public static string Substitute(string str, Dictionary<string, int> orderTable, params object[] vars)
		{
			if(str == null)
				throw new ArgumentNullException("This function takes a string as the first parameter.");

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
		}*/

		/// <summary>
		/// Format the specified str in the way like the printf of the C language does.
		/// </summary>
		/// <param name='str'>
		/// The string containing formats.
		/// </param>
		/// <param name='vars'>
		/// Variables.
		/// </param>
        [ExpressoFunction("format")]
		public static string Format(string str, params object[] vars)
		{
			if(str == null)
                throw new ArgumentNullException(nameof(str));

			string tmp = str;
			int i = 0;
            tmp = formatRefs.Replace(tmp, m => {
				if(m.Groups[2].Value == "b"){
					if(!(vars[i] is int))
                        throw new PanickedException("Can not format objects in binary except an integer!");

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
		/// Iterates over some sequence and returns a tuple containing an index and the corresponding element for each
		/// element in the source sequence.
		/// </summary>
        public static IEnumerable<Tuple<int, T>> Each<T>(IEnumerable<T> src)
		{
			var er = src.GetEnumerator();
			if(!er.MoveNext())
				throw new InvalidOperationException();

			for(int i = 0; er.MoveNext(); ++i)
                yield return new Tuple<int, T>(i, er.Current);
		}
		#endregion
	}
}
