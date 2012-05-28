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
	}
}

