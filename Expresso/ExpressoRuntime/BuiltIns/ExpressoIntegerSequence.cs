using System;
using System.Collections;
using System.Collections.Generic;

using Expresso.Runtime;

namespace Expresso.Builtins
{
	/// <summary>
	/// Expresso組み込みのIntSeqオブジェクト。
	/// The IntSeq object, which represents a sequence of integers.
	/// As such, it can be used like the "slice" operation in Python.
	/// I mean, we have some sequence named <c>seq</c> and an expression like <c>seq[[1..5]]</c>
	/// returns a new sequence which holds the elements from #1 to #5 of the original sequence
	/// (<c>seq</c> this time).
	/// </summary>
	[ExpressoType("intseq")]
	public class ExpressoIntegerSequence : IEnumerable<object>
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
		
        public IEnumerator<object> Val{
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
		
		public ExpressoIntegerSequence(int start, int end, int inputStep)
		{
			this.lower = start;
			this.upper = end;
			this.step = inputStep;
		}

		internal static ExpressoIntegerSequence Make(int start, int end, int step = 1)
		{
			return new ExpressoIntegerSequence(start, end, step);
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
			ExpressoIntegerSequence seq;
			int next;
			int current;
			
            object IEnumerator.Current{
                get{
					return this.Current;
				}
			}
			
            public object Current{
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
		/// Checks whether the integer sequence includes the specified n or not.
		/// </summary>
		/// <param name='n'>
		/// <c>true</c>; if n is in the sequence; otherwise, <c>false</c>.
		/// </param>
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
		public bool IsSequential()
		{
			return this.step == 1;
		}
	}
}

