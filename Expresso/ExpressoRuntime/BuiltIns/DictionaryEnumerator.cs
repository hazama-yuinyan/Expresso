using System;
using System.Collections;
using System.Collections.Generic;

namespace Expresso.Runtime.Builtins
{
    /// <summary>
    /// IEnumerator adaptor in Expresso for Dictionary.
    /// </summary>
    public class DictionaryEnumerator<K, V> : IEnumerator<Tuple<K, V>>
    {
        IEnumerator<KeyValuePair<K, V>> enumerator;

        public DictionaryEnumerator(IDictionary<K, V> dict)
        {
            enumerator = dict.GetEnumerator();
        }

        public Tuple<K, V> Current => new Tuple<K, V>(enumerator.Current.Key, enumerator.Current.Value);

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset();
        }
    }
}
