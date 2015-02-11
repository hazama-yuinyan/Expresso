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
        /// Given two iterators, it 
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
		public static string Format(string str, params object[] vars)
		{
			if(str == null)
				throw new ArgumentNullException("str");

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
        /// Wrapper method for Console.Write.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        [ExpressoFunction("print")]
        public static void Print(string format, params object[] args)
        {
            if(format == null)
                throw new ArgumentNullException("format");

            /*int i = 0;
            string tmp = FormatRefs.Replace(format, m => {

            });

            Console.Write(tmp);*/
        }

        /// <summary>
        /// Println the specified format and args.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        [ExpressoFunction("println")]
        public static void Println(string format, params object[] args)
        {
            if(format == null)
                throw new ArgumentNullException("format");
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

	public sealed class BuiltinNativeMethods
	{
		static BuiltinNativeMethods inst = null;

		//Dictionary<string, Dictionary<string, NativeFunction>> native_methods;

		BuiltinNativeMethods()
		{
			/*var list = new Dictionary<string, NativeFunction>{
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
				{"constructor", new NativeLambdaTernary("constructor", Helpers.MakeArg(new Identifier("path", new TypeAnnotation(ObjectTypes.STRING))),
					                                        Helpers.MakeArg(new Identifier("option", new TypeAnnotation(ObjectTypes.STRING))),
					                                        Helpers.MakeArg(new Identifier("encoding", new TypeAnnotation(ObjectTypes.STRING)), new Constant{ValType = ObjectTypes.STRING, Value = "UTF-8"}),
					                                        (object path, object option, object encoding) => FileObject.OpenFile((string)path, (string)option, (string)encoding))},
				{"read", new NativeFunctionNAry("read", Helpers.MakeNativeMethodCall(typeof(FileObject), "Read"))},
				{"readLine", new NativeFunctionNAry("readLine", Helpers.MakeNativeMethodCall(typeof(FileObject), "ReadLine"))},
				{"readAll", new NativeFunctionNAry("readAll", Helpers.MakeNativeMethodCall(typeof(FileObject), "ReadAll"))},
				{"write", new NativeFunctionNAry("write", Helpers.MakeNativeMethodCall(typeof(FileObject), "Write", typeof(object)))},
				{"close", new NativeFunctionNAry("close", Helpers.MakeNativeMethodCall(typeof(FileObject), "Dispose"))},
				{"openFile", new NativeLambdaTernary("openFile", Helpers.MakeArg(new Identifier("path", new TypeAnnotation(ObjectTypes.STRING), null, 1)),
					                                     Helpers.MakeArg(new Identifier("option", new TypeAnnotation(ObjectTypes.STRING), null, 2)),
					                                     Helpers.MakeArg(new Identifier("encoding", new TypeAnnotation(ObjectTypes.STRING), null, 3), new Constant{ValType = ObjectTypes.STRING, Value = "UTF-8"}),
					                                     (object path, object option, object encoding) => FileObject.OpenFile((string)path, (string)option, (string)encoding))}
			};

			native_methods = new Dictionary<string, Dictionary<string, NativeFunction>>{
				{"List", list},
				{"Tuple", tuple},
				{"Dictionary", dict},
				{"File", file_obj}
			};*/
		}

		public static BuiltinNativeMethods Instance()
		{
			if(inst == null)
				inst = new BuiltinNativeMethods();

			return inst;
		}

		/*public NativeFunction LookupMethod(string typeName, string methodName)
		{
			Dictionary<string, NativeFunction> type_dict;
			if(!native_methods.TryGetValue(typeName, out type_dict))
				throw ExpressoOps.InvalidTypeError("{0} is not a native class name!", typeName);

			NativeFunction method;
			if(!type_dict.TryGetValue(methodName, out method))
				throw ExpressoOps.ReferenceError("{0} doesn't have the method \"{1}\".", typeName, methodName);

			return method;
		}*/
	}
}

