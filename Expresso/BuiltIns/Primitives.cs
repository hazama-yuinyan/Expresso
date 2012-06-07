using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Expresso.BuiltIns
{
	/// <summary>
	/// Expressoの組み込み型（数値型、文字列型など）。
	/// The primitives for Expresso.
	/// </summary>
	public class ExpressoPrimitive : ExpressoObj
	{
		public object Value{get; internal set;}
	}

	#region Expressoの組み込みオブジェクト型郡
	/// <summary>
	/// ExpressoのRangeオブジェクト。
	/// The range object, which represents a range of integers.
	/// </summary>
	public class ExpressoRange : ExpressoObj, IEnumerable<int>
	{
		/// <summary>
		/// 範囲の下限
		/// The lower bound.
		/// </summary>
		private int _start;
		
		/// <summary>
		/// 範囲の上限。-1のときは無限リストを生成する
		/// The upper bound. When set to -1, it generates a infinite series of list.
		/// </summary>
		private int _end;
		
		/// <summary>
		/// ステップ
		/// The step by which the iteration proceeds.
		/// </summary>
		private int _step;

		public IEnumerator<int> Range
		{
			get{
				int i = this._start;
				while (true) {
					if(this._end != -1 && i >= this._end)
						yield break;

					yield return i;
					i += this._step;
				}
			}
		}
		
		public ExpressoRange(int start, int end, int step)
		{
			this._start = start;
			this._end = end;
			this._step = step;
		}
		
		public List<object> Take(int count)
		{
			var tmp = new List<object>(count);
			for (int i = 0; i < count; ++i) {
				tmp.Add(Range);
			}
			
			return tmp;
		}

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<int> GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		public struct Enumerator : IEnumerator<int>, IEnumerator
		{
			private ExpressoRange range;
			private int next;
			private int current;

			object IEnumerator.Current
			{
				get
				{
					if(this.next < 0)
						throw new InvalidOperationException();

					return this.Current;
				}
			}

			public int Current
			{
				get
				{
					this.current = this.next;
					return this.current;
				}
			}

			internal Enumerator(ExpressoRange range)
			{
				this.range = range;
				this.next = range._start;
				this.current = range._start;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				this.next = this.range._start;
			}

			public bool MoveNext()
			{
				if(this.next < 0)
					return false;

				if(this.range._end == -1 || this.next < this.range._end){
					this.next += this.range._step;
					return true;
				}

				this.next = -1;
				return false;
			}
		}
	}

	public interface ExpressoContainer : IEnumerable
	{
		int Size();
		bool Empty();
		bool Contains(object obj);
	}

	/// <summary>
	/// Expresso組み込みのtupleオブジェクト。
	/// The built-in Tuple.
	/// </summary>
	public class ExpressoTuple : ExpressoObj, ExpressoContainer
	{
		/// <summary>
		/// tupleの中身
		/// </summary>
		private List<ExpressoObj> _contents;
		
		public List<ExpressoObj> Contents{get{return this._contents;}}
		
		public ExpressoTuple(List<ExpressoObj> contents)
		{
			this._contents = contents;
		}

		#region ExpressoContainer implementations
		public int Size()
		{
			return _contents.Count;
		}

		public bool Empty()
		{
			return _contents.Count == 0;
		}

		public bool Contains(object obj)
		{
			var expresso_obj = obj as ExpressoObj;
			if(expresso_obj == null)
				return false;

			return _contents.Contains(expresso_obj);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _contents.GetEnumerator();
		}
		#endregion
	}

	public class ExpressoArray : ExpressoObj, ExpressoContainer
	{
		public ExpressoObj[] Contents{get; internal set;}

		#region ExpressoContainer implementations
		public int Size()
		{
			return Contents.Length;
		}

		public bool Empty()
		{
			return Contents.Length == 0;
		}

		public bool Contains(object obj)
		{
			var expresso_obj = obj as ExpressoObj;
			if(expresso_obj == null)
				return false;

			foreach (var item in Contents) {
				if(item == expresso_obj)
					return true;
			}

			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Contents.GetEnumerator();
		}
		#endregion
	}
	
	/// <summary>
	/// Expressoの組み込みListクラス。
	/// The built-in List class.
	/// </summary>
	public class ExpressoList : ExpressoObj, ExpressoContainer
	{
		public List<object> Contents{get; internal set;}

		#region ExpressoContainer implementations
		public int Size()
		{
			return Contents.Count;
		}

		public bool Empty()
		{
			return Contents.Count == 0;
		}

		public bool Contains(object obj)
		{
			if(obj == null)
				return false;

			return Contents.Contains(obj);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Contents.GetEnumerator();
		}
		#endregion
	}
	
	/// <summary>
	/// Expressoの組み込みDictionaryクラス。
	/// The built-in Dictionary class.
	/// </summary>
	public class ExpressoDict : ExpressoObj, ExpressoContainer
	{
		public Dictionary<ExpressoObj, object> Contents{get; internal set;}

		#region ExpressoContainer implementations
		public int Size()
		{
			return Contents.Count;
		}

		public bool Empty()
		{
			return Contents.Count == 0;
		}

		public bool Contains(object obj)
		{
			var expresso_obj = obj as ExpressoObj;
			if(expresso_obj == null)
				return false;

			return Contents.ContainsKey(expresso_obj);
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return Contents.GetEnumerator();
		}
		#endregion
	}
	#endregion
	
	/// <summary>
	/// グローバルな環境で開かれているファイルの情報を管理する。
	/// プログラム中でただひとつ存在するように、シングルトンによる実装になっている。
	/// This class manages just one file on a program.
	/// </summary>
	public class FileWrapper : ExpressoObj, IDisposable
	{
		private StreamReader _reader = null;
		private StreamWriter _writer = null;
		
		private FileWrapper(){}
		
		static FileWrapper inst = null;
		
		static public FileWrapper GetInstance()
		{
			if(inst == null)
				inst = new FileWrapper();
			
			return inst;
		}
		
		void OpenFile(string path, FileAccess access, bool append)
		{
			if(_reader != null && access == FileAccess.Read || access == FileAccess.ReadWrite)
				_reader.Dispose();
			else if(_writer != null && access == FileAccess.Write || access == FileAccess.ReadWrite)
				_writer.Dispose();
			
			if(access == FileAccess.Read){
				_reader = new StreamReader(path);
			}else if(access == FileAccess.Write){
				_writer = new StreamWriter(path, append);
			}else{
				_reader = new StreamReader(path);
				_writer = new StreamWriter(path, append);
			}
		}
		
		public void Dispose()
		{
			if(_reader != null)
				_reader.Dispose();
			
			if(_writer != null)
				_writer.Dispose();
		}
	}
}

