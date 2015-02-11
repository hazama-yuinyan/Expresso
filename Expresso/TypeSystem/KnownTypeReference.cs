using System;


namespace Expresso.TypeSystem
{
    /// <summary>
    /// Represents some well-known types.
    /// </summary>
    public enum KnownTypeCode : byte
    {
        // Note: DefaultResolvedTypeDefinition uses (KnownTypeCode)-1 as special value for "not yet calculated".
        // The order of type codes at the beginning must correspond to those in System.TypeCode.

        /// <summary>
        /// Not one of the known types.
        /// </summary>
        None = 0x00,        //00000000
        /// <summary><c>bool</c> (System.Boolean)</summary>
        Bool = 0x01,        //00000001
        /// <summary><c>byte</c> (System.Byte)</summary>
        Byte = 0x02,        //00000010
        /// <summary><c>char</c> ()</summary>
        Char = 0x03,        //00000011
        /// <summary><c>int</c> (System.Int32)</summary>
        Int = 0x04,         //00000100,
        /// <summary><c>uint</c> (System.UInt32)</summary>
        UInt = 0x05,        //00000101
        /// <summary><c>float</c> (System.Single)</summary>
        Float = 0x06,       //00000110
        /// <summary><c>double</c> (System.Double)</summary>
        Double = 0x07,      //00000111
        /// <summary><c>bigint</c> (System.Math.BigInteger)</summary>
        BigInteger = 0x08,  //00001000
        /// <summary><c>string</c> (System.String)</summary>
        String = 0x09,

        // String was the last element from System.TypeCode, now our additional known types start

        /// <summary>System.Tuple</summary>
        Tuple = 0x10,
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

