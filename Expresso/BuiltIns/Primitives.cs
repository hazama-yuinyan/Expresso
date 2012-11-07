using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Expresso.Ast;
using Expresso.Interpreter;
using Expresso.Helpers;

namespace Expresso.BuiltIns
{
	#region Expressoの組み込みオブジェクト型郡
	/// <summary>
	/// Expresso組み込みのIntSeqオブジェクト。
	/// The IntSeq object, which represents a sequence of integers.
	/// As such, it can be used like the "slice" operation in Python.
	/// I mean, we have some sequence named <c>seq</c> and an expression like <c>seq[1..5]</c>
	/// returns a new sequence which holds the elements from #1 to #5 of the original sequence
	/// (<c>seq</c> this time).
	/// </summary>
	public class ExpressoIntegerSequence : IEnumerable<object>, SequenceGenerator<List<object>, object>
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
		/// The step by which the iteration proceeds at a time.
		/// </summary>
		private int _step;

		public IEnumerator<object> Val
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

		public TYPES Type{
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

		public IEnumerator<object> GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region SequenceGenerator implementation
		public object Generate()
		{
			return -1;
		}

		public List<object> Take(int count)
		{
			var objs = new List<object>(count);
			var enumerator = Val;
			for(int i = 0; i < count; ++i)
				objs.Add(enumerator.Current);

			return objs;
		}
		#endregion

		#region The enumerator for IntegerSequence
		public struct Enumerator : IEnumerator<object>, IEnumerator
		{
			private ExpressoIntegerSequence seq;
			private int next;
			private int current;

			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			public object Current
			{
				get
				{
					if(next < 0)
						throw new InvalidOperationException();

					this.current = this.next;
					return this.current;
				}
			}

			internal Enumerator(ExpressoIntegerSequence seq)
			{
				this.seq = seq;
				this.next = seq._start - seq._step;
				this.current = -127;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				this.next = this.seq._start - seq._step;
				this.current = -127;
			}

			public bool MoveNext()
			{
				if(this.next == int.MinValue)
					return false;

				if(this.seq._end == -1 || this.next < this.seq._end){
					this.next += this.seq._step;
					return true;
				}

				this.next = int.MinValue;
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
	public interface ExpressoContainer : IEnumerable<object>
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
		object Slice(ExpressoIntegerSequence seq);
	}

	/// <summary>
	/// Expresso組み込みのtupleオブジェクト。
	/// The built-in Tuple.
	/// </summary>
	/// <seealso cref="ExpressoContainer"/>
	public class ExpressoTuple : ExpressoContainer
	{
		private List<object> _contents;

		/// <summary>
		/// Tupleの中身。
		/// The content of the tuple.
		/// </summary>
		/// <value>
		/// The contents.
		/// </value>
		public List<object> Contents{get{return this._contents;}}

		public TYPES Type{get{return TYPES.TUPLE;}}

		public ExpressoTuple(List<object> contents)
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
			return _contents.Contains(obj);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _contents.GetEnumerator();
		}

		public IEnumerator<object> GetEnumerator()
		{
			return _contents.GetEnumerator();
		}

		public object Slice(ExpressoIntegerSequence seq)
		{
			var sliced = new List<object>();
			var enumerator = seq.GetEnumerator();
			if(!enumerator.MoveNext())
				throw new InvalidOperationException();

			int index = (int)enumerator.Current;
			do{
				sliced.Add(Contents[index]);
			}while(enumerator.MoveNext() && (index = (int)enumerator.Current) < Contents.Count);

			return new ExpressoTuple(sliced);
		}
		#endregion

		public object AccessMember(object subscription)
		{
			if(subscription is int){
				int index = (int)subscription;
				return Contents[index];
			}else if(subscription is string){
				return null;
			}
			return null;
		}
	}

	/// <summary>
	/// The built-in fraction class, which represents a fraction as it is.
	/// </summary>
	public class ExpressoFraction : IComparable
	{
		/// <summary>
		/// Represents the dominator of the fraction.
		/// </summary>
		/// <value>
		/// The denominator.
		/// </value>
		public ulong Denominator{get; internal set;}

		/// <summary>
		/// Represents the numerator of the fraction.
		/// </summary>
		/// <value>
		/// The numerator.
		/// </value>
		public ulong Numerator{get; internal set;}

		/// <summary>
		/// Indicates whether the fraction is positive or not.
		/// </summary>
		/// <value>
		/// <c>true</c> if this object represents a positive fraction; otherwise, <c>false</c>.
		/// </value>
		public bool IsPositive{get; internal set;}

		/// <summary>
		/// Initializes a new instance of the <see cref="Expresso.BuiltIns.ExpressoFraction"/> class.
		/// </summary>
		/// <param name='numerator'>
		/// Numerator.
		/// </param>
		/// <param name='denominator'>
		/// Denominator.
		/// </param>
		/// <param name='isPositive'>
		/// Is positive.
		/// </param>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="Expresso.BuiltIns.ExpressoFraction"/> class with a floating-point value.
		/// </summary>
		/// <param name='val'>
		/// The value in double.
		/// </param>
		public ExpressoFraction(double val)
		{
			long floored = (long)val, denominator = 1;
			double tmp = val - (double)floored;
			while(tmp < 1.0){
				tmp *= 10.0;
				floored *= 10;
				denominator *= 10;
			}

			this.IsPositive = tmp > 0 ? true : false;
			this.Numerator = (ulong)Math.Abs((long)tmp + floored);
			this.Denominator = (ulong)denominator;
			this.Reduce();
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

		/// <summary>
		/// Returns the inverse of the fraction.
		/// </summary>
		public ExpressoFraction GetInverse()
		{
			return new ExpressoFraction(Denominator, Numerator, IsPositive);
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		public ExpressoFraction Copy()
		{
			return new ExpressoFraction(Numerator, Denominator, IsPositive);
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
			return string.Format("[ExpressoFraction: {0}{1} / {2}]", IsPositive ? "" : "-", Numerator, Denominator);
		}

		#region Arithmetic operators
		public static ExpressoFraction operator+(ExpressoFraction lhs, ExpressoFraction rhs)
		{
			if(object.Equals(lhs.Denominator, rhs.Denominator)){
				return new ExpressoFraction(lhs.Numerator + rhs.Numerator, lhs.Denominator);
			}else{
				ExpressoFraction tmp = lhs.Copy();
				ExpressoFraction other_reduced = tmp.Reduce(tmp);
				tmp.Numerator = tmp.Numerator + other_reduced.Numerator;
				return tmp;
			}
		}
		
		public static ExpressoFraction operator+(ExpressoFraction lhs, ulong rhs)
		{
			return new ExpressoFraction(lhs.Numerator + rhs * lhs.Denominator, lhs.Denominator, lhs.IsPositive);
		}

		public static ExpressoFraction operator+(ExpressoFraction lhs, long rhs)
		{
			var rhs_numerator = rhs * (long)lhs.Denominator;
			return new ExpressoFraction((rhs > 0) ? lhs.Numerator + (ulong)rhs_numerator : lhs.Numerator - (ulong)rhs_numerator, lhs.Denominator,
			                            lhs.IsPositive);
		}
		
		public static ExpressoFraction operator+(ExpressoFraction lhs, double rhs)
		{
			ExpressoFraction tmp = lhs.Copy(), other = new ExpressoFraction(rhs);
			ExpressoFraction new_self = other.Reduce(tmp);
			new_self.Numerator = new_self.Numerator + other.Numerator;
			return new_self;
		}

		/// <param name='src'>
		/// Source.
		/// </param>
		/// <remarks>The unary operator minus.</remarks>
		public static ExpressoFraction operator-(ExpressoFraction src)
		{
			return new ExpressoFraction(src.Numerator, src.Denominator, !src.IsPositive);
		}

		public static ExpressoFraction operator-(ExpressoFraction lhs, ExpressoFraction rhs)
		{
			return lhs + (-rhs);
		}

		public static ExpressoFraction operator-(ExpressoFraction lhs, ulong rhs)
		{
			var long_rhs = (long)rhs;
			return lhs + (-long_rhs);
		}

		public static ExpressoFraction operator-(ExpressoFraction lhs, long rhs)
		{
			return lhs + (-rhs);
		}

		public static ExpressoFraction operator-(ExpressoFraction lhs, double rhs)
		{
			return lhs + (-rhs);
		}
		
		public static ExpressoFraction operator*(ExpressoFraction lhs, ExpressoFraction rhs)
		{
			return new ExpressoFraction(lhs.Numerator * rhs.Numerator, lhs.Denominator * rhs.Denominator,
			                            (lhs.IsPositive && rhs.IsPositive || !lhs.IsPositive && !rhs.IsPositive) ? true : false);
		}
		
		public static ExpressoFraction operator*(ExpressoFraction lhs, ulong rhs)
		{
			return new ExpressoFraction(lhs.Numerator * rhs, lhs.Denominator, lhs.IsPositive);
		}

		public static ExpressoFraction operator*(ExpressoFraction lhs, long rhs)
		{
			return new ExpressoFraction(lhs.Numerator * (ulong)rhs, lhs.Denominator,
			                            (lhs.IsPositive && rhs > 0 || !lhs.IsPositive && rhs < 0) ? true : false);
		}
		
		public static ExpressoFraction operator*(ExpressoFraction lhs, double rhs)
		{
			var other = new ExpressoFraction(rhs);
			return lhs * other;
		}

		public static ExpressoFraction operator/(ExpressoFraction lhs, ExpressoFraction rhs)
		{
			var rhs_inversed = rhs.GetInverse();
			return lhs * rhs_inversed;
		}

		public static ExpressoFraction operator/(ExpressoFraction lhs, ulong rhs)
		{
			var rhs_inversed = new ExpressoFraction(1, rhs, true);
			return lhs * rhs_inversed;
		}

		public static ExpressoFraction operator/(ExpressoFraction lhs, long rhs)
		{
			var rhs_is_positive = rhs > 0;
			var rhs_inversed = new ExpressoFraction(1, (ulong)rhs, rhs_is_positive);
			return lhs * rhs_inversed;
		}

		public static ExpressoFraction operator/(ExpressoFraction lhs, double rhs)
		{
			var rhs_inversed = new ExpressoFraction(rhs).GetInverse();
			return lhs * rhs_inversed;
		}
		#endregion

		#region Comparison operators
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
		#endregion

		public static explicit operator double(ExpressoFraction src)
		{
			double result = (double)src.Numerator / (double)src.Denominator;
			if(!src.IsPositive)
				result = -result;

			return result;
		}
	}

	/// <summary>
	/// Expressoの組み込み型の一つ、Expression型。基本的にはワンライナーのクロージャーだが、
	/// 記号演算もサポートする。
	/// The built-in expression class. It is, in most cases, just a function object
	/// with one line of code, but may contain symbolic expression. Therefore,
	/// it has the capability of symbolic computation.
	/// </summary>
	public class ExpressoExpression
	{
		public TYPES Type{get{return TYPES.EXPRESSION;}}
	}
	#endregion
	
	/// <summary>
	/// グローバルな環境で開かれているファイルの情報を管理する。
	/// プログラム中でただひとつ存在するように、シングルトンによる実装になっている。
	/// This class manages just one file on a program.
	/// </summary>
	public class FileWrapper : IDisposable
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
