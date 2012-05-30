using System;
using System.Collections.Generic;

namespace Expresso.BuiltIns
{
	/// <summary>
	/// Expressoの組み込み関数郡。
	/// </summary>
	public static class ExpressoFunctions
	{
		public static List<object> Take(ExpressoRange range, int count)
		{
			return range.Take(count);
		}
		
		public static ExpressoTuple MakeTuple(List<ExpressoObj> objs)
		{
			return new ExpressoTuple(objs);
		}
		
		public static ExpressoList MakeList(List<object> objs)
		{
			return new ExpressoList{Contents = objs};
		}
		
		public static ExpressoDict MakeDict(List<ExpressoObj> keys, List<object> values, int count)
		{
			Dictionary<ExpressoObj, object> tmp = new Dictionary<ExpressoObj, object>(count);
			for (int i = 0; i < count; ++i) {
				tmp.Add(keys[i], values[i]);
			}
			return new ExpressoDict{Contents = tmp};
		}
	}
}

