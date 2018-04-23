//
// Utilities.cs
//
// Author:
//       train12 <kotonechan@live.jp>
//
// Copyright (c) 2018 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Numerics;

namespace Expresso.Runtime.Utils
{
    public static class Utilities
    {
        /// <summary>
        /// Calculates the GCD(Greatest common deivisor).
        /// </summary>
        /// <remarks>
        /// Because it uses <see cref="BigInteger"/>, it can be slow.
        /// </remarks>
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
                if(r == 0)
                    break;
                
                last = r;
                a = (b > r) ? b : r; b = (b > r) ? r : b;
            }

            return last;
        }

        /// <summary>
        /// Calculates the LCM(Least common multiple).
        /// </summary>
        /// <remarks>
        /// Because it uses <see cref="CalcGCD(BigInteger, BigInteger)"/> which uses <see cref="BigInteger"/>,
        /// it can be slow.
        /// </remarks>
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
