using System;
using System.Collections;
using System.Collections.Generic;


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
    /// If you do need another copy of the array, then consider using the clone method.
	/// </summary>
	[ExpressoType("intseq")]
    public class ExpressoIntegerSequence : IEnumerable<int>
	{
		/// <summary>
		/// 数列の開始点。
		/// The lower bound.
		/// </summary>
		int lower;
		
		/// <summary>
		/// 数列の終点。int.MinValueのときは無限リストを生成する。
		/// The upper bound. When set to "int.MinValue", it generates an infinite series of list.
		/// Note that the upper bound will not be included in the resulting sequence.
		/// </summary>
		int upper;
		
		/// <summary>
		/// ステップ。
		/// The step by which the iteration proceeds at a time.
		/// </summary>
		int step;
		
        public IEnumerator<int> Val{
			get{
				int i = this.lower;
				while(true){
					if(this.upper != -1 && i >= this.upper)
						yield break;
					
					yield return i;
					i += this.step;
				}
			}
		}
		
        public ExpressoIntegerSequence(int start, int end, int inputStep, bool includesUpper)
		{
			this.lower = start;
            this.upper = includesUpper ? end + 1 : end;
			this.step = inputStep;
		}

        internal static ExpressoIntegerSequence Make(int start, int end, int step = 1, bool includesUpper = false)
		{
            return new ExpressoIntegerSequence(start, end, step, includesUpper);
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
			int next;
			int current;
			
            object IEnumerator.Current{
                get{
					return this.Current;
				}
			}
			
            public int Current{
                get{
					if(next == int.MinValue)
						throw new InvalidOperationException();
					
					this.current = this.next;
					return this.current;
				}
			}
			
			internal Enumerator(ExpressoIntegerSequence seq)
			{
				this.seq = seq;
				this.next = seq.lower - seq.step;
				this.current = -127;
			}
			
			public void Dispose()
			{
			}
			
			void IEnumerator.Reset()
			{
				this.next = this.seq.lower - seq.step;
				this.current = -127;
			}
			
			public bool MoveNext()
			{
				if(this.next == int.MinValue)
					return false;
				
				if(this.seq.upper == int.MinValue || this.next + this.seq.step < this.seq.upper){
					this.next += this.seq.step;
					return true;
				}
				
				this.next = int.MinValue;
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
        [ExpressoFunction("includes")]
		public bool Includes(int n)
		{
			var remaining = n % this.step;
			return remaining - this.lower == 0;
		}
		
		/// <summary>
		/// Determines whether the sequence is sequential.
		/// </summary>
		/// <returns>
		/// <c>true</c>; if the sequence is sequential, that is, the step is 1; otherwise, <c>false</c>.
		/// </returns>
        [ExpressoFunction("isSequential")]
		public bool IsSequential()
		{
			return this.step == 1;
		}

        public override string ToString()
        {
            return string.Format("{0}..{1}:{2}", lower, upper, step);
        }
	}
}

