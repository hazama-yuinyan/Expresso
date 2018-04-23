using System;
using System.Numerics;

using Expresso.Runtime.Utils;

namespace Expresso.Runtime.Builtins
{
	/// <summary>
	/// The built-in fraction class, which represents a fraction as it is.
	/// </summary>
	public struct Fraction : IComparable
	{
		/// <summary>
		/// Represents the dominator of the fraction.
		/// </summary>
		public BigInteger Denominator{get; internal set;}
		
		/// <summary>
		/// Represents the numerator of the fraction.
		/// </summary>
		public BigInteger Numerator{get; internal set;}
		
		/// <summary>
		/// Indicates whether the fraction is positive or not.
		/// </summary>
		/// <value>
		/// <c>true</c> if this object represents a positive fraction; otherwise, <c>false</c>.
		/// </value>
		public bool IsPositive{
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
		public Fraction(BigInteger numerator, BigInteger denominator) : this()
		{
			var denom_is_negative = denominator < 0;
			Denominator = BigInteger.Abs(denominator);
			Numerator = denom_is_negative ? -numerator : numerator;
			this.Reduce();
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Expresso.Runtime.Builtins.Fraction"/> class with an integer.
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
		/// Initializes a new instance of the <see cref="Expresso.Runtime.Builtins.Fraction"/> class with a floating-point value.
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
			var gcd = Utilities.CalcGCD(BigInteger.Abs(Numerator), Denominator);
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
			var lcm = Utilities.CalcLCM(Denominator, other.Denominator);
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
		
		#region Arithmetic operators
		public static Fraction operator+(Fraction lhs, Fraction rhs)
		{
			if(lhs.Denominator == rhs.Denominator){
				return new Fraction(lhs.Numerator + rhs.Numerator, lhs.Denominator);
			}else{
				Fraction tmp = lhs;
				Fraction other_reduced = tmp.Reduce(rhs);
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
			Fraction tmp = lhs, other = new Fraction(rhs);
			Fraction new_self = other.Reduce(tmp);
			new_self.Numerator = new_self.Numerator + other.Numerator;
			return new_self;
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
		
		public static Fraction operator%(Fraction lhs, Fraction rhs)
		{
			var tmp_rhs = rhs;
			var tmp_lhs = lhs;
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
}

