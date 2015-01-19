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
        /// <summary><c>bool</c> (System.Boolean)</summary>
        Bool,
        /// <summary><c>byte</c> (System.Byte)</summary>
        Byte,
        /// <summary><c>char</c> </summary>
        Char,
        /// <summary><c>int</c> (System.Int32)</summary>
        Int,
        /// <summary><c>uint</c> (System.UInt32)</summary>
        UInt,
        /// <summary><c>float</c> (System.Single)</summary>
        Float,
        /// <summary><c>double</c> (System.Double)</summary>
        Double,
        /// <summary> <c>bigint</c> (System.Math.BigInteger)</summary>
        BigInteger,

        /// <summary><c>string</c> (System.String)</summary>
        String = 18,

        // String was the last element from System.TypeCode, now our additional known types start

        /// <summary>System.Tuple</summary>
        Tuple,
        /// <summary>System.Collections.Generics.List{T}</summary>
        Vector,
        /// <summary>System.Collections.Generics.Dictionary{K, V}</summary>
        Dictionary,
        /// <summary><c>void</c> (System.Void)</summary>
        Void,
        /// <summary><c>System.Array</c></summary>
        Array,
        /// <summary><c>The function type</c></summary>
        Function,
        /// <summary>The integer sequence type</summary>
        IntSeq,
        /// <summary><c>The iterator type in the Expresso's standard library</c></summary>
        Iterator
    }

    public class KnownTypeReference
    {
        public KnownTypeReference()
        {
        }
    }
}

