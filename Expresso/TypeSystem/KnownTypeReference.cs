using System;


namespace Expresso.TypeSystem
{
    /// <summary>
    /// Represents some well-known types.
    /// </summary>
    public enum KnownTypeCode
    {
        // Note: DefaultResolvedTypeDefinition uses (KnownTypeCode)-1 as special value for "not yet calculated".
        // The order of type codes at the beginning must correspond to those in System.TypeCode.

        /// <summary>
        /// Not one of the known types.
        /// </summary>
        None,
        /// <summary><c>object</c> (System.Object)</summary>
        Object,
        /// <summary><c>bool</c> (System.Boolean)</summary>
        Boolean,
        /// <summary><c>byte</c> (System.Byte)</summary>
        Byte,
        /// <summary><c>int</c> (System.Int32)</summary>
        Int,
        /// <summary><c>float</c> (System.Single)</summary>
        Float,
        /// <summary><c>double</c> (System.Double)</summary>
        Double,
        /// <summary><c>decimal</c> (System.Decimal)</summary>
        Decimal,
        /// <summary> <c>bigint</c> (System.Math.BigInteger)</summary>
        BigInteger,

        /// <summary><c>string</c> (System.String)</summary>
        String = 18,

        // String was the last element from System.TypeCode, now our additional known types start

        /// <summary>System.Tuple</summary>
        Tuple,
        /// <summary><c>void</c> (System.Void)</summary>
        Void,
        /// <summary><c>System.Type</c></summary>
        Type,
        /// <summary><c>System.Array</c></summary>
        Array,
        /// <summary><c>System.Attribute</c></summary>
        Attribute,
        /// <summary><c>System.ValueType</c></summary>
        ValueType,
        /// <summary><c>System.Enum</c></summary>
        Enum,
        /// <summary><c>The function type</c></summary>
        Function,
        /// <summary><c>The expression type</c></summary>
        Expression,
        /// <summary><c>System.Exception</c></summary>
        Exception,
        /// <summary><c>System.IntPtr</c></summary>
        IEnumerable,
        /// <summary><c>System.Collections.IEnumerator</c></summary>
        IEnumerator,
        /// <summary><c>System.Collections.Generic.IEnumerable{T}</c></summary>
        IEnumerableOfT,
        /// <summary><c>System.Collections.Generic.IEnumerator{T}</c></summary>
        IEnumeratorOfT,
        /// <summary><c>System.Collections.Generic.ICollection</c></summary>
        ICollection,
        /// <summary><c>System.Collections.Generic.ICollection{T}</c></summary>
        ICollectionOfT,
        /// <summary><c>System.Collections.Generic.IList</c></summary>
        IList,
        /// <summary><c>System.Collections.Generic.IList{T}</c></summary>
        IListOfT,
        /// <summary><c>System.Collections.Generic.IReadOnlyCollection{T}</c></summary>
        IReadOnlyCollectionOfT,
        /// <summary><c>System.Collections.Generic.IReadOnlyList{T}</c></summary>
        IReadOnlyListOfT,
        /// <summary><c>System.Threading.Tasks.Task</c></summary>
        Task,
        /// <summary><c>System.Threading.Tasks.Task{T}</c></summary>
        TaskOfT,
        /// <summary><c>System.Nullable{T}</c></summary>
        NullableOfT,
        /// <summary><c>System.IDisposable</c></summary>
        IDisposable,
        /// <summary><c>System.Runtime.CompilerServices.INotifyCompletion</c></summary>
        INotifyCompletion,
        /// <summary><c>System.Runtime.CompilerServices.ICriticalNotifyCompletion</c></summary>
        ICriticalNotifyCompletion
    }

    public class KnownTypeReference
    {
        public KnownTypeReference()
        {
        }
    }
}

