using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Expresso.Runtime.Builtins
{
	/// <summary>
	/// Expresso組み込みのIntSeqオブジェクト。
	/// The `intseq` type, which represents a sequence of integers.
	/// As such, it can be used like the "slice" operation in Python.
	/// I mean, we have some sequence named <c>seq</c> and an expression like <c>seq[1..5]</c>
	/// returns a new sequence which holds the elements from #2 to #6 of the original sequence
	/// (<c>seq</c> this time).
    /// Note that an `intseq` object affecting on sequence types doesn't create a new copy
    /// of the original sequence. It just returns an iterator that views into the original sequence.
    /// If you do need another copy of the array, then consider using the System.Linq methods.
	/// </summary>
    public class ExpressoIntegerSequence : IEnumerable<int>
	{
		/// <summary>
		/// 数列の開始点。
		/// The start value.
		/// </summary>
        int start;
		
		/// <summary>
		/// 数列の終点。int.MinValueのときは無限リストを生成する。
		/// The end value. When set to "int.MinValue", it generates an infinite series of list.
		/// Note that the upper bound will not be included in the resulting sequence.
		/// </summary>
        int end;
		
		/// <summary>
		/// ステップ。
		/// The step by which the iteration proceeds at a time.
		/// </summary>
		int step;
		
        public IEnumerator<int> Val{
			get{
				int i = start;
				while(true){
					if(end != -1 && i >= end)
						yield break;
					
					yield return i;
					i += step;
				}
			}
		}
		
        public ExpressoIntegerSequence(int start, int end, int step, bool includesUpper)
		{
			this.start = start;
            this.end = includesUpper ? end + 1 : end;
            this.step = step;
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
		
		#region The enumerator for IntegerSequence
        public struct Enumerator : IEnumerator<int>, IEnumerator
		{
			ExpressoIntegerSequence seq;
			int current, next;
			
            object IEnumerator.Current{
                get{
					return Current;
				}
			}
			
            public int Current{
                get{
					if(next == int.MinValue)
						throw new InvalidOperationException();
					
					current = next;
					return current;
				}
			}
			
			internal Enumerator(ExpressoIntegerSequence seq)
			{
				this.seq = seq;
				next = seq.start - seq.step;
				current = -127;
			}
			
			public void Dispose()
			{
			}
			
			void IEnumerator.Reset()
			{
				next = seq.start - seq.step;
				current = -127;
			}
			
			public bool MoveNext()
			{
				if(next == int.MinValue)
					return false;
				
                if(seq.end == int.MinValue || Math.Abs(next + seq.step) < Math.Abs(seq.end)){
					next += seq.step;
					return true;
				}
				
				next = int.MinValue;
				return false;
			}
		}
		#endregion
		
		/// <summary>
        /// Checks whether `n` will be included in the generated integer sequence or not.
		/// </summary>
		/// <param name='n'>
		/// <c>true</c>; if n is in the sequence; otherwise, <c>false</c>.
		/// </param>
        /// <remarks>
        /// Note that it checks exact equality, not range inclusion.
        /// In other words, this method will find the answer for the following equation.
        /// n = k * x + a where n is an element of the resulting sequence,
        /// k is the step and a is the lower bound.
        /// </remarks>
		public bool Includes(int n)
		{
			var remaining = n % step;
            return start <= n && n < end && remaining - start == 0;
		}
		
		/// <summary>
		/// Determines whether the sequence is sequential.
		/// </summary>
		/// <returns>
		/// <c>true</c>; if the sequence is sequential, that is, the step is 1; otherwise, <c>false</c>.
		/// </returns>
		public bool IsSequential()
		{
			return step == 1;
		}

        public override string ToString()
        {
            return string.Format("{0}..{1}:{2}", start, end, step);
        }

        /// <summary>
        /// Expands the intseq to an array.
        /// </summary>
        /// <returns>The array from int seq.</returns>
        /// <param name="intSeq">Int seq.</param>
        public static int[] CreateArrayFromIntSeq(ExpressoIntegerSequence intSeq)
        {
            return intSeq.Select(s => s)
                         .ToArray();
        }

        /// <summary>
        /// Expands the intseq to a list.
        /// </summary>
        /// <returns>The list from int seq.</returns>
        /// <param name="intSeq">Int seq.</param>
        public static List<int> CreateListFromIntSeq(ExpressoIntegerSequence intSeq)
        {
            return intSeq.Select(s => s)
                         .ToList();
        }
	}
}

