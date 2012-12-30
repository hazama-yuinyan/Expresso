using System;
using System.Collections.Generic;
using System.Collections;

using Expresso.Builtins;
using Expresso.Runtime.Exceptions;

namespace Expresso.Runtime.Operations
{
	/// <summary>
	/// Contains functions that are called directly from
	/// generated code to perform low-level runtime functionality.
	/// </summary>
	public static class ExpressoOps
	{
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

		public static int CompareArrays(object[] data0, int size0, object[] data1, int size1, IComparer comparer) {
			int size = Math.Min(size0, size1);
			for (int i = 0; i < size; i++) {
				int c = comparer.Compare(data0[i], data1[i]);
				if (c != 0) return c;
			}
			if (size0 == size1) return 0;
			return size0 > size1 ? +1 : -1;
		}
	}
}

