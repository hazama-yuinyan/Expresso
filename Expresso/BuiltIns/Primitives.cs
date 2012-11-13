using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		/// 数列の終点。int.MinValueのときは無限リストを生成する。
		/// The upper bound. When set to "int.MinValue", it generates an infinite series of list.
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
			for(int i = 0; enumerator.MoveNext() && i < count; ++i)
				objs.Add(enumerator.Current);

			return objs;
		}

		public List<object> TakeAll()
		{
			if(_end == int.MinValue)
				throw new EvalException("Can not take all elements from an infinite series of list!");

			var er = new Enumerator(this);
			var tmp = new List<object>();
			while(er.MoveNext())
				tmp.Add(er.Current);

			return tmp;
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
					if(next == int.MinValue)
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

				if(this.seq._end == int.MinValue || this.next < this.seq._end){
					this.next += this.seq._step;
					return true;
				}

				this.next = int.MinValue;
				return false;
			}
		}
		#endregion

		/// <summary>
		/// Checks whether the integer sequence includes the specified n or not.
		/// </summary>
		/// <param name='n'>
		/// <c>true</c>; if n is in the sequence; otherwise, <c>false</c>.
		/// </param>
		public bool Includes(int n)
		{
			var remaining = n % this._step;
			return remaining - this._start == 0;
		}

		/// <summary>
		/// Determines whether the sequence is sequential.
		/// </summary>
		/// <returns>
		/// <c>true</c>; if the sequence is sequential, that is, the step is 1; otherwise, <c>false</c>.
		/// </returns>
		public bool IsSequential()
		{
			return this._step == 1;
		}

		static public ExpressoClass.ExpressoObj Construct(ExpressoIntegerSequence inst)
		{
			var privates = new Dictionary<string, int>();
			privates.Add("start", 0);
			privates.Add("end", 1);
			privates.Add("step", 2);

			var publics = new Dictionary<string, int>();
			publics.Add("includes", 3);
			publics.Add("isSequential", 4);
			publics.Add("take", 5);
			publics.Add("generate", 6);
			publics.Add("takeAll", 7);

			var definition = new ExpressoClass.ClassDefinition("IntSeq", privates, publics);
			definition.Members = new object[]{
				inst._start,
				inst._end,
				inst._step,
				new NativeFunctionUnary<bool, int>(
					"includes", new Argument{Name = "n", ParamType = TYPES.INTEGER}, inst.Includes
				),
				new NativeFunctionNullary<bool>(
					"isSequential", inst.IsSequential
				),
				new NativeFunctionUnary<List<object>, int>(
					"take", new Argument{Name = "count", ParamType = TYPES.INTEGER}, inst.Take
				),
				new NativeFunctionNullary<object>(
					"generate", inst.Generate
				),
				new NativeFunctionNullary<List<object>>(
					"takeAll", inst.TakeAll
				)
			};

			return new ExpressoClass.ExpressoObj(definition, TYPES.SEQ);
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
		int Size{get;}

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
		private object[] _content;

		/// <summary>
		/// Tupleの中身。
		/// The content of the tuple.
		/// </summary>
		/// <value>
		/// The contents.
		/// </value>
		public object[] Content{get{return this._content;}}

		public ExpressoTuple(object[] content)
		{
			this._content = content;
		}

		public ExpressoTuple(List<object> content)
		{
			this._content = content.ToArray();
		}

		public object this[int index]
		{
			get{
				return _content[index];
			}
		}

		#region ExpressoContainer implementations
		public int Size
		{
			get{
				return _content.Length;
			}
		}

		public bool Empty()
		{
			return _content.Length == 0;
		}

		public bool Contains(object obj)
		{
			return _content.Contains(obj);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _content.GetEnumerator();
		}

		public IEnumerator<object> GetEnumerator()
		{
			return new Enumerator(this);
		}

		public object Slice(ExpressoIntegerSequence seq)
		{
			var sliced = new List<object>();
			var enumerator = seq.GetEnumerator();
			if(!enumerator.MoveNext())
				throw new InvalidOperationException();

			int index = (int)enumerator.Current;
			do{
				sliced.Add(Content[index]);
			}while(enumerator.MoveNext() && (index = (int)enumerator.Current) < Content.Length);

			return new ExpressoTuple(sliced);
		}
		#endregion

		#region The enumerator for ExpressoTuple
		public struct Enumerator : IEnumerator<object>, IEnumerator
		{
			private IEnumerator er;

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
					return er.Current;
				}
			}

			internal Enumerator(ExpressoTuple tuple)
			{
				this.er = tuple.GetEnumerator();
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				er.Reset();
			}

			public bool MoveNext()
			{
				return er.MoveNext();
			}
		}
		#endregion

		static public ExpressoClass.ExpressoObj Construct(ExpressoTuple inst)
		{
			var privates = new Dictionary<string, int>();
			privates.Add("content", 0);

			var publics = new Dictionary<string, int>();
			publics.Add("length", 1);
			publics.Add("empty", 2);
			publics.Add("contains", 3);

			var definition = new ExpressoClass.ClassDefinition("Tuple", privates, publics);
			definition.Members = new object[]{
				inst,
				new NativeFunctionNullary<int>(
					"length", () => inst.Size
				),
				new NativeFunctionNullary<bool>(
					"empty", inst.Empty
				),
				new NativeFunctionUnary<bool, object>(
					"contains", new Argument{Name = "elem", ParamType = TYPES.VAR}, inst.Contains
				)
			};

			return new ExpressoClass.ExpressoObj(definition, TYPES.TUPLE);
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

		/// <summary>
		/// 約分を行う。
		/// </summary>
		public ExpressoFraction Reduce()
		{
			var gdc = ImplementaionHelpers.CalcGDC(Numerator, Denominator);
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
			var lcm = ImplementaionHelpers.CalcLCM(Denominator, other.Denominator);
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

		public override string ToString()
		{
			return string.Format("[ExpressoFraction: {0}{1} / {2}]", IsPositive ? "" : "-", Numerator, Denominator);
		}

		public ExpressoFraction Power(ExpressoFraction y)
		{
			return new ExpressoFraction(0, 0, true);
		}

		public ExpressoFraction Power(long y)
		{
			var tmp_y = new ExpressoFraction(y);
			return Power(tmp_y);
		}

		public ExpressoFraction Power(ulong y)
		{
			var tmp_y = new ExpressoFraction(y);
			return Power(tmp_y);
		}

		public ExpressoFraction Power(double y)
		{
			var tmp_y = new ExpressoFraction(y);
			return Power(tmp_y);
		}

		public ExpressoFraction Power(object y)
		{
			if(y is ExpressoFraction)
				return Power((ExpressoFraction)y);
			else if(y is long)
				return Power((long)y);
			else if(y is ulong)
				return Power((ulong)y);
			else if(y is double)
				return Power((double)y);
			else
				throw new EvalException("The power operation cann't take that type of object as the right operand.");
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

		public static ExpressoFraction operator+(ExpressoFraction lhs, object rhs)
		{
			if(rhs is ExpressoFraction)
				return lhs + (ExpressoFraction)rhs;
			else if(rhs is long)
				return lhs + (long)rhs;
			else if(rhs is ulong)
				return lhs + (ulong)rhs;
			else if(rhs is double)
				return lhs + (double)rhs;
			else
				throw new EvalException("Can not apply the + operator on that type of object.");
		}

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

		public static ExpressoFraction operator-(ExpressoFraction lhs, object rhs)
		{
			if(rhs is ExpressoFraction)
				return lhs - (ExpressoFraction)rhs;
			else if(rhs is long)
				return lhs - (long)rhs;
			else if(rhs is ulong)
				return lhs - (ulong)rhs;
			else if(rhs is double)
				return lhs - (double)rhs;
			else
				throw new EvalException("Can not apply the - operator on that type of object.");
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

		public static ExpressoFraction operator*(ExpressoFraction lhs, object rhs)
		{
			if(rhs is ExpressoFraction)
				return lhs * (ExpressoFraction)rhs;
			else if(rhs is long)
				return lhs * (long)rhs;
			else if(rhs is ulong)
				return lhs * (ulong)rhs;
			else if(rhs is double)
				return lhs * (double)rhs;
			else
				throw new EvalException("Can not apply the * operator on that type of object.");
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

		public static ExpressoFraction operator/(ExpressoFraction lhs, object rhs)
		{
			if(rhs is ExpressoFraction)
				return lhs / (ExpressoFraction)rhs;
			else if(rhs is long)
				return lhs / (long)rhs;
			else if(rhs is ulong)
				return lhs / (ulong)rhs;
			else if(rhs is double)
				return lhs / (double)rhs;
			else
				throw new EvalException("Can not apply the / operator on that type of object.");
		}

		public static ExpressoFraction operator%(ExpressoFraction lhs, ExpressoFraction rhs)
		{
			var tmp_rhs = rhs.Copy();
			var tmp_lhs = lhs.Copy();
			if(lhs.Denominator != rhs.Denominator)
				tmp_lhs = tmp_rhs.Reduce(lhs);

			long lhs_numerator = (tmp_lhs.IsPositive) ? (long)tmp_lhs.Numerator : -(long)tmp_lhs.Numerator;
			long rhs_numerator = (tmp_rhs.IsPositive) ? (long)tmp_rhs.Numerator : -(long)tmp_rhs.Numerator;
			var remaining = lhs_numerator % rhs_numerator;
			return new ExpressoFraction((ulong)remaining, tmp_lhs.Denominator, remaining > 0);
		}

		public static ExpressoFraction operator%(ExpressoFraction lhs, ulong rhs)
		{
			var rhs_fraction = new ExpressoFraction(rhs);
			return lhs % rhs_fraction;
		}

		public static ExpressoFraction operator%(ExpressoFraction lhs, long rhs)
		{
			var rhs_fraction = new ExpressoFraction(rhs);
			return lhs % rhs_fraction;
		}

		public static ExpressoFraction operator%(ExpressoFraction lhs, double rhs)
		{
			var rhs_fraction = new ExpressoFraction(rhs);
			return lhs % rhs_fraction;
		}

		public static ExpressoFraction operator%(ExpressoFraction lhs, object rhs)
		{
			if(rhs is ExpressoFraction)
				return lhs % (ExpressoFraction)rhs;
			else if(rhs is long)
				return lhs % (long)rhs;
			else if(rhs is ulong)
				return lhs % (ulong)rhs;
			else if(rhs is double)
				return lhs % (double)rhs;
			else
				throw new EvalException("Can not apply the % operator on that type of object.");
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
