using System;
using System.Numerics;

namespace Expresso.Utils
{
	/// <summary>
	/// ユーティリティーメソッド郡。
	/// Utils.
	/// </summary>
	public static class Utilities
	{
		public static ArgumentNullException MakeArgumentItemNullException(int index, string arrayName)
		{
			return new ArgumentNullException(String.Format("{0}[{1}]", arrayName, index));
		}

		/// <summary>
		/// Calculates the GCD(Greatest common deivisor).
		/// </summary>
		/// <returns>
		/// The GCD of the two BigIntegers.
		/// </returns>
		/// <param name='first'>
		/// First.
		/// </param>
		/// <param name='second'>
		/// Second.
		/// </param>
		public static BigInteger CalcGCD(BigInteger first, BigInteger second)
		{
			BigInteger r, a = (first > second) ? first : second, b = (first > second) ? second : first, last = b;
			while(true){
				r = a - b;
				if(r == 0) break;
				last = r;
				a = (b > r) ? b : r; b = (b > r) ? r : b;
			}
			
			return last;
		}
		
		/// <summary>
		/// Calculates the LCM(Least common multiple).
		/// </summary>
		/// <returns>
		/// The LCM of the two BigIntegers.
		/// </returns>
		/// <param name='first'>
		/// First.
		/// </param>
		/// <param name='second'>
		/// Second.
		/// </param>
		public static BigInteger CalcLCM(BigInteger first, BigInteger second)
		{
			BigInteger gcd = CalcGCD(first, second);
			return first * second / gcd;
		}
	}
}

