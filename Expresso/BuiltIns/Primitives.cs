using System;
using System.Collections.Generic;

namespace Expresso.BuiltIns
{
	/// <summary>
	/// Expressoの組み込み型（数値型、文字列型など）。
	/// </summary>
	public class ExpressoPrimitive : ExpressoObj
	{
		public object Value{get; internal set;}
	}
	
	/// <summary>
	/// Expresso組み込みのtupleオブジェクト。
	/// </summary>
	public class ExpressoTuple : ExpressoObj
	{
		private List<ExpressoObj> contents;
		
		public List<ExpressoObj> Contents{get{return this.contents;}}
		
		public ExpressoTuple(List<ExpressoObj> contents)
		{
			this.contents = contents;
		}
	}
	
	/// <summary>
	/// ExpressoのRangeオブジェクト。
	/// </summary>
	public class ExpressoRange : ExpressoObj
	{
		/// <summary>
		/// 範囲の下限
		/// </summary>
		private int start;
		
		/// <summary>
		/// 範囲の上限。-1のときは無限リストを生成する
		/// </summary>
		private int end;
		
		/// <summary>
		/// ステップ
		/// </summary>
		private int step;
		
		public IEnumerable<int> Range
		{
			get{
				int i = start;
				while (true) {
					if(end != -1 && i > end)
						yield break;
					
					i += step;
					yield return i;
				}
			}
		}
		
		public ExpressoRange(int start, int end, int step)
		{
			this.start = start;
			this.end = end;
			this.step = step;
		}
		
		public List<object> Take(int count)
		{
			var tmp = new List<object>();
			for (int i = 0; i < count; ++i) {
				tmp.Add(Range);
			}
			
			return tmp;
		}
	}
}

