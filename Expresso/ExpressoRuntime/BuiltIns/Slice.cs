using System;
using System.Collections;
using System.Collections.Generic;

namespace Expresso.Runtime.Builtins
{
    /// <summary>
    /// The Slice object which views a portion of some sequence using an Enumerator.
    /// </summary>
    public class Slice<T, S> : IEnumerable<S>
        where T: IList<S>
    {
        T collection;
        ExpressoIntegerSequence int_seq;

        public Slice(T collection, ExpressoIntegerSequence intSeq)
        {
            this.collection = collection;
            int_seq = intSeq;
        }

        #region Implementation of IEnumerable<T>
        public IEnumerator<S> GetEnumerator()
        {
            return new Enumerator(collection, int_seq.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Enumerator for Slice
        public struct Enumerator : IEnumerator<S>, IEnumerator
        {
            T collection;
            IEnumerator<int> int_seq;
            S current, next;

            public S Current{
                get{
                    current = next;
                    return current;
                }
            }

            object IEnumerator.Current => Current;

            public Enumerator(T collection, IEnumerator<int> enumerator)
            {
                this.collection = collection;
                int_seq = enumerator;
                current = default(S);
                next = default(S);
            }

            public void Dispose()
            {
                int_seq.Dispose();
            }

            public bool MoveNext()
            {
                var move_next = int_seq.MoveNext();
                if(move_next)
                    this.next = collection[int_seq.Current];

                return move_next;
            }

            public void Reset()
            {
                ((IEnumerator)int_seq).Reset();
            }
        }
        #endregion

        public override string ToString()
        {
            return string.Format("[Slice target={0} range={1}]", collection, int_seq);
        }
    }
}
