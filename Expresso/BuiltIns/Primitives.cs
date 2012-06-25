using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Expresso.Interpreter;
using Expresso.Helpers;

namespace Expresso.BuiltIns
{
	/// <summary>
	/// Expressoの組み込み型（数値型、文字列型など）。
	/// The primitives for Expresso.
	/// </summary>
	public class ExpressoPrimitive : ExpressoObj
	{
		public object Value{get; internal set;}

		/// <summary>
		/// The type of the stored value.
		/// </summary>
		public override TYPES Type{get; internal set;}

		public override bool Equals(object obj)
		{
			var other = obj as ExpressoPrimitive;
			if(other == null) return false;

			return Type == other.Type && Value.Equals(other.Value);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	#region Expressoの組み込みオブジェクト型郡
	/// <summary>
	/// Expresso組み込みのIntSeqオブジェクト。
	/// The IntSeq object, which represents a sequence of integers.
	/// As such, it can be used like the "slice" operation in Python.
	/// I mean, we have some sequence named <c>seq</c> and an expression like <c>seq[1..5]</c>
	/// returns a new sequence which holds the elements from #1 to #5 of the original sequence
	/// (<c>seq</c> this time).
	/// </summary>
	public class ExpressoIntegerSequence : ExpressoObj, IEnumerable<ExpressoPrimitive>, SequenceGenerator<ExpressoList, ExpressoPrimitive>
	{
		/// <summary>
		/// 数列の開始点。
		/// The lower bound.
		/// </summary>
		private int _start;
		
		/// <summary>
		/// 数列の終点。-1のときは無限リストを生成する。
		/// The upper bound. When set to -1, it generates a infinite series of list.
		/// Note that the upper bound will not be included in the resulting sequence.
		/// </summary>
		private int _end;
		
		/// <summary>
		/// ステップ。
		/// The step by which the iteration proceeds.
		/// </summary>
		private int _step;

		public IEnumerator<int> Val
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

		public override TYPES Type{
			get{
				return TYPES.SEQ;
			}
		}
		
		public ExpressoIntegerSequence(int start, int end, int step)
		{
			this._start = start;
			this._end = end;
			this._step = step;
		}
		
		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<ExpressoPrimitive> GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region SequenceGenerator implementation
		public ExpressoPrimitive Generate()
		{
			return null;
		}

		public ExpressoList Take(int count)
		{
			var objs = new List<ExpressoObj>(count);
			var enumerator = Val;
			for(int i = 0; i < count; ++i){
				var tmp = new ExpressoPrimitive{Value = enumerator.Current, Type = TYPES.INTEGER};
				objs.Add(tmp);
			}

			return ExpressoFunctions.MakeList(objs);
		}
		#endregion

		#region The enumerator for IntegerSequence
		public struct Enumerator : IEnumerator<ExpressoPrimitive>, IEnumerator
		{
			private ExpressoIntegerSequence seq;
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

			public ExpressoPrimitive Current
			{
				get
				{
					this.current = this.next;
					return new ExpressoPrimitive{Value = this.current, Type = TYPES.INTEGER};
				}
			}

			internal Enumerator(ExpressoIntegerSequence seq)
			{
				this.seq = seq;
				this.next = seq._start;
				this.current = seq._start;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				this.next = this.seq._start;
			}

			public bool MoveNext()
			{
				if(this.next < 0)
					return false;

				if(this.seq._end == -1 || this.next < this.seq._end){
					this.next += this.seq._step;
					return true;
				}

				this.next = -1;
				return false;
			}
		}
		#endregion

		public bool Includes(int n)
		{
			var remaining = n % this._step;
			return remaining - this._start == 0;
		}

		public bool IsSequential()
		{
			return this._step == 1;
		}
	}

	/// <summary>
	/// Expressoのコンテナ型のベースクラス。
	/// The base class for Expresso's container classes.
	/// </summary>
	public interface ExpressoContainer : IEnumerable
	{
		/// <summary>
		/// このコンテナのサイズを返す。
		/// Returns the number of elements the container has.
		/// </summary>
		int Size();

		/// <summary>
		/// このコンテナが空かどうか返す。
		/// Determines whether the container is empty or not.
		/// </summary>
		bool Empty();

		/// <summary>
		/// このコンテナ内に指定された要素が存在するかどうか調べる。
		/// Inspects whether the container has a specific element.
		/// </summary>
		bool Contains(object obj);

		/// <summary>
		/// IntegerSequenceを使ってコンテナの一部の要素をコピーした新しいコンテナを生成する。
		/// Do the "slice" operation on the container with an IntegerSequence.
		/// </summary>
		ExpressoObj Slice(ExpressoIntegerSequence seq);
	}

	/// <summary>
	/// Expresso組み込みのtupleオブジェクト。
	/// The built-in Tuple.
	/// </summary>
	/// <seealso cref="ExpressoContainer"/>
	public class ExpressoTuple : ExpressoObj, ExpressoContainer
	{
		/// <summary>
		/// tupleの中身
		/// </summary>
		private List<ExpressoObj> _contents;
		
		public List<ExpressoObj> Contents{get{return this._contents;}}

		public override TYPES Type{get{return TYPES.TUPLE;}}
		
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

		public ExpressoObj Slice(ExpressoIntegerSequence seq)
		{
			var sliced = new List<ExpressoObj>();
			var enumerator = seq.GetEnumerator();
			int index = (int)enumerator.Current.Value;
			do{
				sliced.Add(Contents[index]);
			}while(enumerator.MoveNext() && (index = (int)enumerator.Current.Value) < Contents.Count);

			return new ExpressoTuple(sliced);
		}
		#endregion

		public override ExpressoObj AccessMember(ExpressoObj subscription)
		{
			if(!ImplementaionHelpers.IsOfType(subscription, TYPES.INTEGER))
				throw new EvalException("The expression can not be evaluated to an int.");

			int index = (int)((ExpressoPrimitive)subscription).Value;
			return Contents[index];
		}
	}

	/// <summary>
	/// Expressoの組み込み配列。
	/// The Built-in Array.
	/// </summary>
	/// <seealso cref="ExpressoContainer"/>
	public class ExpressoArray : ExpressoObj, ExpressoContainer
	{
		public ExpressoObj[] Contents{get; internal set;}

		public override TYPES Type{get{return TYPES.ARRAY;}}

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

		public ExpressoObj Slice(ExpressoIntegerSequence seq)
		{
			var sliced = new List<ExpressoObj>();
			var enumerator = seq.GetEnumerator();
			int index = (int)enumerator.Current.Value;
			do{
				sliced.Add(Contents[index]);
			}while(enumerator.MoveNext() && (index = (int)enumerator.Current.Value) < Contents.Length);

			return new ExpressoArray{Contents = sliced.ToArray()};
		}
		#endregion

		public override ExpressoObj AccessMember(ExpressoObj subscription)
		{
			if(!ImplementaionHelpers.IsOfType(subscription, TYPES.INTEGER))
				throw new EvalException("The expression can not be evaluated to an int.");

			int index = (int)((ExpressoPrimitive)subscription).Value;
			return Contents[index];
		}
	}
	
	/// <summary>
	/// Expressoの組み込みListクラス。
	/// The built-in List class.
	/// </summary>
	/// <seealso cref="ExpressoContainer"/>
	public class ExpressoList : ExpressoObj, ExpressoContainer
	{
		public List<ExpressoObj> Contents{get; internal set;}

		public override TYPES Type{get{return TYPES.LIST;}}

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

			var tmp = obj as ExpressoObj;
			if(tmp == null)
				throw new EvalException("Invalid object!");

			return Contents.Contains(tmp);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Contents.GetEnumerator();
		}

		public ExpressoObj Slice(ExpressoIntegerSequence seq)
		{
			var sliced = new List<ExpressoObj>();
			var enumerator = seq.GetEnumerator();
			int index = (int)enumerator.Current.Value;
			do{
				sliced.Add(Contents[index]);
			}while(enumerator.MoveNext() && (index = (int)enumerator.Current.Value) < Contents.Count);

			return new ExpressoList{Contents = sliced};
		}
		#endregion

		public override ExpressoObj AccessMember(ExpressoObj subscription)
		{
			if(!ImplementaionHelpers.IsOfType(subscription, TYPES.INTEGER))
				throw new EvalException("The expression can not be evaluated to an int.");

			int index = (int)((ExpressoPrimitive)subscription).Value;
			return Contents[index];
		}
	}
	
	/// <summary>
	/// Expressoの組み込みDictionaryクラス。
	/// The built-in Dictionary class.
	/// </summary>
	/// <seealso cref="ExpressoContainer"/>
	public class ExpressoDict : ExpressoObj, ExpressoContainer
	{
		public Dictionary<ExpressoObj, ExpressoObj> Contents{get; internal set;}

		public override TYPES Type{get{return TYPES.DICT;}}

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

		public ExpressoObj Slice(ExpressoIntegerSequence seq)
		{
			return null;
		}
		#endregion

		public override ExpressoObj AccessMember(ExpressoObj subscription)
		{
			ExpressoObj obj;
			if(!Contents.TryGetValue(subscription, out obj))
				throw new EvalException("The dictionary doesn't have a value corresponding to {0}", subscription.ToString());

			return obj;
		}
	}

	/// <summary>
	/// The built-in fraction class, which represents a fraction as it is.
	/// </summary>
	public class ExpressoFraction : ExpressoObj, IComparable
	{
		/// <summary>
		/// Gets or sets the denominator.
		/// </summary>
		/// <value>
		/// The denominator.
		/// </value>
		public ulong Denominator{get; internal set;}

		/// <summary>
		/// Gets or sets the numerator.
		/// </summary>
		/// <value>
		/// The numerator.
		/// </value>
		public ulong Numerator{get; internal set;}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is positive.
		/// </summary>
		/// <value>
		/// <c>true</c> if this object represents a positive fraction; otherwise, <c>false</c>.
		/// </value>
		public bool IsPositive{get; internal set;}

		public ExpressoFraction(ulong numerator, ulong denominator, bool isPositive = true)
		{
			Denominator = denominator;
			Numerator = numerator;
			IsPositive = isPositive;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Expresso.BuiltIns.ExpressoFraction"/> class with an integer.
		/// </summary>
		/// <param name='integer'>
		/// Integer.
		/// </param>
		public ExpressoFraction(long integer)
		{
			Denominator = 1;
			Numerator = (ulong)Math.Abs(integer);
			IsPositive = integer > 0 ? true : false;
		}

		private static ulong CalcGDC(ulong first, ulong second)
		{
			ulong r, a = (first > second) ? first : second, b = (first > second) ? second : first, last = b;
			while(true){
				r = a - b;
				if(r == 0) break;
				last = r;
				a = (b > r) ? b : r; b = (b > r) ? r : b;
			}
			
			return last;
		}
		
		private static ulong CalcLCM(ulong first, ulong second)
		{
			ulong gdc = CalcGDC(first, second);
			return first * second / gdc;
		}

		/// <summary>
		/// 約分を行う。
		/// </summary>
		public ExpressoFraction Reduce()
		{
			var gdc = CalcGDC(Numerator, Denominator);
			Numerator /= gdc;
			Denominator /= gdc;
			return this;
		}

		/// <summary>
		/// 通分を行う。
		/// </summary>
		/// <param name='other'>
		/// 通分をする対象。
		/// </param>
		public ExpressoFraction Reduce(ExpressoFraction other)
		{
			var lcm = CalcLCM(Denominator, other.Denominator);
			Numerator *= lcm / Denominator;
			return new ExpressoFraction(other.Numerator * lcm / other.Denominator, lcm, other.IsPositive);
		}

		public int CompareTo(object obj)
		{
			var other = obj as ExpressoFraction;

			if(other == null)
				return 0xffff;

			return -1;
		}

		public override bool Equals(object obj)
		{
			var other = obj as ExpressoFraction;

			if(other == null)
				return false;

			return this == other;
		}

		public override int GetHashCode()
		{
			return Numerator.GetHashCode() ^ Denominator.GetHashCode() ^ IsPositive.GetHashCode();
		}

		public override string ToString ()
		{
			return string.Format ("[ExpressoFraction: {0}{1} / {2}]", IsPositive ? "" : "-", Numerator, Denominator);
		}

		public static bool operator>(ExpressoFraction lhs, ExpressoFraction rhs)
		{
			if(lhs.IsPositive && !rhs.IsPositive)
				return true;
			else if(!lhs.IsPositive && rhs.IsPositive)
				return false;
			else if(lhs.Denominator == rhs.Denominator)
				return lhs.Numerator > rhs.Numerator;

			var rhs_reduced = lhs.Reduce(rhs);
			return lhs.Numerator > rhs_reduced.Numerator;
		}

		public static bool operator<(ExpressoFraction lhs, ExpressoFraction rhs)
		{
			if(lhs.IsPositive && !rhs.IsPositive)
				return false;
			else if(!lhs.IsPositive && rhs.IsPositive)
				return true;
			else if(lhs.Denominator == rhs.Denominator)
				return lhs.Numerator < rhs.Numerator;

			var rhs_reduced = lhs.Reduce(rhs);
			return lhs.Numerator < rhs_reduced.Numerator;
		}

		public static bool operator==(ExpressoFraction lhs, ExpressoFraction rhs)
		{
			if(lhs.IsPositive != rhs.IsPositive) return false;
			return (lhs.Numerator == rhs.Numerator && lhs.Denominator == rhs.Denominator);
		}

		public static bool operator!=(ExpressoFraction lhs, ExpressoFraction rhs)
		{
			return !(lhs == rhs);
		}
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

	/// <summary>
	/// Expressoの組み込み型の一つ、Expression型。基本的にはワンライナーのクロージャーだが、
	/// 記号演算もサポートする。
	/// The built-in expression class. It is, in most cases, just a function object
	/// with one line of code, but may contain symbolic expression. Therefore,
	/// it has the capability of symbolic computation.
	/// </summary>
	public class ExpressoExpression : ExpressoObj
	{
		public override TYPES Type{get{return TYPES.EXPRESSION;}}
	}
}

