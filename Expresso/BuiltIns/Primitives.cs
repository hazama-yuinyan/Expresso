using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.IO;
using Expresso.Ast;
using Expresso.Interpreter;
using Expresso.Helpers;

namespace Expresso.Builtins
{
	#region Expressoの組み込みオブジェクト型郡
	/// <summary>
	/// Expresso組み込みのIntSeqオブジェクト。
	/// The IntSeq object, which represents a sequence of integers.
	/// As such, it can be used like the "slice" operation in Python.
	/// I mean, we have some sequence named <c>seq</c> and an expression like <c>seq[[1..5]]</c>
	/// returns a new sequence which holds the elements from #1 to #5 of the original sequence
	/// (<c>seq</c> this time).
	/// </summary>
	public class ExpressoIntegerSequence : IEnumerable<object>
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

				if(this.seq._end == int.MinValue || this.next + this.seq._step < this.seq._end){
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
				this.er = tuple._content.GetEnumerator();
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
	}

	/// <summary>
	/// The built-in fraction class, which represents a fraction as it is.
	/// </summary>
	public struct Fraction : IComparable
	{
		/// <summary>
		/// Represents the dominator of the fraction.
		/// </summary>
		/// <value>
		/// The denominator.
		/// </value>
		public BigInteger Denominator{get; internal set;}

		/// <summary>
		/// Represents the numerator of the fraction.
		/// </summary>
		/// <value>
		/// The numerator.
		/// </value>
		public BigInteger Numerator{get; internal set;}

		/// <summary>
		/// Indicates whether the fraction is positive or not.
		/// </summary>
		/// <value>
		/// <c>true</c> if this object represents a positive fraction; otherwise, <c>false</c>.
		/// </value>
		public bool IsPositive
		{
			get{
				return Numerator.Sign > 0;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Expresso.Builtins.Fraction"/> class.
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
		public Fraction(BigInteger numerator, BigInteger denominator) : this()
		{
			Denominator = denominator;
			Numerator = numerator;
			this.Reduce();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Expresso.Builtins.Fraction"/> class with an integer.
		/// </summary>
		/// <param name='integer'>
		/// Integer.
		/// </param>
		public Fraction(BigInteger integer) : this()
		{
			Denominator = 1;
			Numerator = integer;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Expresso.Builtins.Fraction"/> class with a floating-point value.
		/// </summary>
		/// <param name='val'>
		/// The value in double.
		/// </param>
		public Fraction(double val) : this()
		{
			BigInteger floored = new BigInteger(val), denominator = new BigInteger(1);
			double tmp = val - (double)floored;
			while(tmp < 1.0){
				tmp *= 10.0;
				floored *= 10;
				denominator *= 10;
			}

			this.Numerator = (BigInteger)tmp + floored;
			this.Denominator = denominator;
			this.Reduce();
		}

		/// <summary>
		/// 約分を行う。
		/// </summary>
		public Fraction Reduce()
		{
			var gcd = ImplementationHelpers.CalcGCD(Numerator, Denominator);
			Numerator /= gcd;
			Denominator /= gcd;
			return this;
		}

		/// <summary>
		/// 通分を行う。
		/// </summary>
		/// <param name='other'>
		/// 通分をする対象。
		/// </param>
		public Fraction Reduce(Fraction other)
		{
			var lcm = ImplementationHelpers.CalcLCM(Denominator, other.Denominator);
			Numerator *= lcm / Denominator;
			return new Fraction(other.Numerator * lcm / other.Denominator, lcm);
		}

		/// <summary>
		/// Returns the inverse of the fraction.
		/// </summary>
		public Fraction GetInverse()
		{
			return new Fraction(Denominator, Numerator);
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		public Fraction Copy()
		{
			return new Fraction(Numerator, Denominator);
		}

		public int CompareTo(object obj)
		{
			if(!(obj is Fraction))
				return -1;

			return -1;
		}

		public override bool Equals(object obj)
		{
			if(!(obj is Fraction))
				return false;

			return this == (Fraction)obj;
		}

		public override int GetHashCode()
		{
			return Numerator.GetHashCode() ^ Denominator.GetHashCode() ^ IsPositive.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("({0} / {1})", Numerator, Denominator);
		}

		public Fraction Power(Fraction y)
		{
			return new Fraction(0, 0);
		}

		public Fraction Power(long y)
		{
			var tmp_y = new Fraction((BigInteger)y);
			return Power(tmp_y);
		}

		public Fraction Power(BigInteger y)
		{
			var tmp_y = new Fraction(y);
			return Power(tmp_y);
		}

		public Fraction Power(double y)
		{
			var tmp_y = new Fraction(y);
			return Power(tmp_y);
		}

		public Fraction Power(object y)
		{
			if(y is Fraction)
				return Power((Fraction)y);
			else if(y is BigInteger)
				return Power((BigInteger)y);
			else if(y is long)
				return Power((long)y);
			else if(y is double)
				return Power((double)y);
			else
				throw new EvalException("The power operation cann't take that type of object as the right operand.");
		}

		#region Arithmetic operators
		public static Fraction operator+(Fraction lhs, Fraction rhs)
		{
			if(object.Equals(lhs.Denominator, rhs.Denominator)){
				return new Fraction(lhs.Numerator + rhs.Numerator, lhs.Denominator);
			}else{
				Fraction tmp = lhs.Copy();
				Fraction other_reduced = tmp.Reduce(tmp);
				tmp.Numerator = tmp.Numerator + other_reduced.Numerator;
				return tmp;
			}
		}
		
		public static Fraction operator+(Fraction lhs, BigInteger rhs)
		{
			return new Fraction(lhs.Numerator + rhs * lhs.Denominator, lhs.Denominator);
		}

		public static Fraction operator+(Fraction lhs, long rhs)
		{
			var rhs_numerator = rhs * lhs.Denominator;
			return new Fraction(lhs.Numerator + rhs_numerator, lhs.Denominator);
		}
		
		public static Fraction operator+(Fraction lhs, double rhs)
		{
			Fraction tmp = lhs.Copy(), other = new Fraction(rhs);
			Fraction new_self = other.Reduce(tmp);
			new_self.Numerator = new_self.Numerator + other.Numerator;
			return new_self;
		}

		public static Fraction operator+(Fraction lhs, object rhs)
		{
			if(rhs is Fraction)
				return lhs + (Fraction)rhs;
			else if(rhs is BigInteger)
				return lhs + (BigInteger)rhs;
			else if(rhs is long)
				return lhs + (long)rhs;
			else if(rhs is double)
				return lhs + (double)rhs;
			else
				throw new EvalException("Can not apply the + operator on that type of object.");
		}

		/// <remarks>The unary operator minus.</remarks>
		public static Fraction operator-(Fraction src)
		{
			return new Fraction(-src.Numerator, src.Denominator);
		}

		public static Fraction operator-(Fraction lhs, Fraction rhs)
		{
			return lhs + (-rhs);
		}

		public static Fraction operator-(Fraction lhs, BigInteger rhs)
		{
			return lhs + (-rhs);
		}

		public static Fraction operator-(Fraction lhs, long rhs)
		{
			return lhs + (-rhs);
		}

		public static Fraction operator-(Fraction lhs, double rhs)
		{
			return lhs + (-rhs);
		}

		public static Fraction operator-(Fraction lhs, object rhs)
		{
			if(rhs is Fraction)
				return lhs - (Fraction)rhs;
			else if(rhs is BigInteger)
				return lhs - (BigInteger)rhs;
			else if(rhs is long)
				return lhs - (long)rhs;
			else if(rhs is double)
				return lhs - (double)rhs;
			else
				throw new EvalException("Can not apply the - operator on that type of object.");
		}
		
		public static Fraction operator*(Fraction lhs, Fraction rhs)
		{
			return new Fraction(lhs.Numerator * rhs.Numerator, lhs.Denominator * rhs.Denominator);
		}
		
		public static Fraction operator*(Fraction lhs, BigInteger rhs)
		{
			return new Fraction(lhs.Numerator * rhs, lhs.Denominator);
		}

		public static Fraction operator*(Fraction lhs, long rhs)
		{
			return new Fraction(lhs.Numerator * rhs, lhs.Denominator);
		}
		
		public static Fraction operator*(Fraction lhs, double rhs)
		{
			var other = new Fraction(rhs);
			return lhs * other;
		}

		public static Fraction operator*(Fraction lhs, object rhs)
		{
			if(rhs is Fraction)
				return lhs * (Fraction)rhs;
			else if(rhs is BigInteger)
				return lhs * (BigInteger)rhs;
			else if(rhs is long)
				return lhs * (long)rhs;
			else if(rhs is double)
				return lhs * (double)rhs;
			else
				throw new EvalException("Can not apply the * operator on that type of object.");
		}

		public static Fraction operator/(Fraction lhs, Fraction rhs)
		{
			var rhs_inversed = rhs.GetInverse();
			return lhs * rhs_inversed;
		}

		public static Fraction operator/(Fraction lhs, BigInteger rhs)
		{
			var rhs_inversed = new Fraction(1, rhs);
			return lhs * rhs_inversed;
		}

		public static Fraction operator/(Fraction lhs, long rhs)
		{
			var rhs_inversed = new Fraction(1, rhs);
			return lhs * rhs_inversed;
		}

		public static Fraction operator/(Fraction lhs, double rhs)
		{
			var rhs_inversed = new Fraction(rhs).GetInverse();
			return lhs * rhs_inversed;
		}

		public static Fraction operator/(Fraction lhs, object rhs)
		{
			if(rhs is Fraction)
				return lhs / (Fraction)rhs;
			else if(rhs is BigInteger)
				return lhs / (BigInteger)rhs;
			else if(rhs is long)
				return lhs / (long)rhs;
			else if(rhs is double)
				return lhs / (double)rhs;
			else
				throw new EvalException("Can not apply the / operator on that type of object.");
		}

		public static Fraction operator%(Fraction lhs, Fraction rhs)
		{
			var tmp_rhs = rhs.Copy();
			var tmp_lhs = lhs.Copy();
			if(lhs.Denominator != rhs.Denominator)
				tmp_lhs = tmp_rhs.Reduce(lhs);

			BigInteger lhs_numerator = tmp_lhs.Numerator, rhs_numerator = tmp_rhs.Numerator;
			var remaining = lhs_numerator % rhs_numerator;
			return new Fraction(remaining, tmp_lhs.Denominator);
		}

		public static Fraction operator%(Fraction lhs, BigInteger rhs)
		{
			var rhs_fraction = new Fraction(rhs);
			return lhs % rhs_fraction;
		}

		public static Fraction operator%(Fraction lhs, long rhs)
		{
			var rhs_fraction = new Fraction((BigInteger)rhs);
			return lhs % rhs_fraction;
		}

		public static Fraction operator%(Fraction lhs, double rhs)
		{
			var rhs_fraction = new Fraction(rhs);
			return lhs % rhs_fraction;
		}

		public static Fraction operator%(Fraction lhs, object rhs)
		{
			if(rhs is Fraction)
				return lhs % (Fraction)rhs;
			else if(rhs is BigInteger)
				return lhs % (BigInteger)rhs;
			else if(rhs is long)
				return lhs % (long)rhs;
			else if(rhs is double)
				return lhs % (double)rhs;
			else
				throw new EvalException("Can not apply the % operator on that type of object.");
		}
		#endregion

		#region Comparison operators
		public static bool operator>(Fraction lhs, Fraction rhs)
		{
			bool lhs_positive = lhs.Numerator > 0, rhs_positive = rhs.Numerator > 0;
			if(lhs_positive && !rhs_positive)
				return true;
			else if(!lhs_positive && rhs_positive)
				return false;
			else if(lhs.Denominator == rhs.Denominator)
				return lhs.Numerator > rhs.Numerator;

			var rhs_reduced = lhs.Reduce(rhs);
			return lhs.Numerator > rhs_reduced.Numerator;
		}

		public static bool operator<(Fraction lhs, Fraction rhs)
		{
			bool lhs_positive = lhs.Numerator > 0, rhs_positive = rhs.Numerator > 0;
			if(lhs_positive && !rhs_positive)
				return false;
			else if(!lhs_positive && rhs_positive)
				return true;
			else if(lhs.Denominator == rhs.Denominator)
				return lhs.Numerator < rhs.Numerator;

			var rhs_reduced = lhs.Reduce(rhs);
			return lhs.Numerator < rhs_reduced.Numerator;
		}

		public static bool operator==(Fraction lhs, Fraction rhs)
		{
			return (lhs.Numerator == rhs.Numerator && lhs.Denominator == rhs.Denominator);
		}

		public static bool operator!=(Fraction lhs, Fraction rhs)
		{
			return !(lhs == rhs);
		}
		#endregion

		public static explicit operator double(Fraction src)
		{
			double result = (double)src.Numerator / (double)src.Denominator;
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
	/*public class ExpressoExpression
	{
		public TYPES Type{get{return TYPES.EXPRESSION;}}

		private Function body;
	}*/
	#endregion
}
