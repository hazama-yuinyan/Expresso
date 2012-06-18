using System;
using System.Collections.Generic;
using Expresso.Helpers;

namespace Expresso.BuiltIns
{
	/// <summary>
	/// Expressoの組み込み関数郡。
	/// The built-in functions for Expresso.
	/// </summary>
	public static class ExpressoFunctions
	{
		#region Expressoの組み込み関数郡
		public static ExpressoList Take(ExpressoIntegerSequence range, int count)
		{
			return range.Take(count);
		}

		public static ExpressoTuple Zip(params ExpressoObj[] objs)
		{
			var tmp = new List<ExpressoObj>(objs);
			return new ExpressoTuple(tmp);
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

