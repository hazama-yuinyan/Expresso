using System;
using System.Collections.Generic;
using Expresso.Ast;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;

namespace Expresso.TypeSystem
{
    using CSharpKnownTypeCode = ICSharpCode.NRefactory.TypeSystem.KnownTypeCode;

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
        /// <summary><c>char</c> (System.Char)</summary>
        Char = 0x03,        //00000011
        /// <summary><c>int</c> (System.Int32)</summary>
        Int = 0x04,         //00000100,
        /// <summary><c>uint</c> (System.UInt32)</summary>
        UInt = 0x05,        //00000101
        /// <summary><c>bigint</c> (System.Numerics.BigInteger)</summary>
        BigInteger = 0x06,  //00000110
        /// <summary><c>float</c> (System.Single)</summary>
        Float = 0x07,       //00000111
        /// <summary><c>double</c> (System.Double)</summary>
        Double = 0x08,      //00001000
        /// <summary><c>string</c> (System.String)</summary>
        String = 0x09,

        // String was the last element from System.TypeCode, now our additional known types start

        /// <summary>System.Tuple</summary>
        Tuple = 0x0a,
        /// <summary>System.Collections.Generics.List&lt;T&gt;</summary>
        Vector,
        /// <summary>System.Collections.Generics.Dictionary&lt;K, V&gt;</summary>
        Dictionary,
        /// <summary><c>void</c> (System.Void)</summary>
        Void,
        /// <summary><c>System.Array</c></summary>
        Array,
        /// <summary>The integer sequence type(Expresso.Runtime.Builtins.ExpressoIntegerSequence)</summary>
        IntSeq,
        /// <summary>The slice type(Expresso.Runtime.Builtins.Slice)</summary>
        Slice,
    }

    public sealed class KnownTypeReference : ITypeReference
    {
        static HashSet<string> _Keywords = new HashSet<string>{
            "if",
            "for",
            "assert",
            "var",
            "let",
            "while",
            "in",
            "match",
            "export",
            "module",
            "import",
            "as",
            "is",
            "class",
            "break",
            "continue",
            "else",
            "true",
            "false",
            "try",
            "catch",
            "finally",
            "null",
            "self",
            "return",
            "def",
            "throw",
            "upto",
            "yield",
            "super",
            "int",
            "uint",
            "bool",
            "float",
            "bigint",
            "tuple",
            "vector",
            "dictionary",
            "byte",
            "char",
            "string",
            "function",
            "intseq",
            "slice"
        };

        public static HashSet<string> Keywords{
            get{return _Keywords;}
        }

        public const int KnownTypeCount = (int)KnownTypeCode.Slice + 1;

        static readonly KnownTypeReference[] knownTypeReferences = {
            null,
            new KnownTypeReference(KnownTypeCode.Bool, "System", "Boolean", baseType: CSharpKnownTypeCode.ValueType),
            new KnownTypeReference(KnownTypeCode.Byte, "System", "Byte", baseType: CSharpKnownTypeCode.ValueType),
            new KnownTypeReference(KnownTypeCode.Char, "System", "Char", baseType: CSharpKnownTypeCode.ValueType),
            new KnownTypeReference(KnownTypeCode.Int, "System", "Int32", baseType: CSharpKnownTypeCode.ValueType),
            new KnownTypeReference(KnownTypeCode.UInt, "System", "UInt32", baseType: CSharpKnownTypeCode.ValueType),
            new KnownTypeReference(KnownTypeCode.BigInteger, "System.Numerics", "BigInteger"),
            new KnownTypeReference(KnownTypeCode.Float, "System", "Single", baseType: CSharpKnownTypeCode.ValueType),
            new KnownTypeReference(KnownTypeCode.Double, "System", "Double", baseType: CSharpKnownTypeCode.ValueType),
            new KnownTypeReference(KnownTypeCode.String, "System", "String"),
            new KnownTypeReference(KnownTypeCode.Tuple, "System", "Tuple"),
            new KnownTypeReference(KnownTypeCode.Vector, "System.Collections.Generic", "List", 1),
            new KnownTypeReference(KnownTypeCode.Dictionary, "System.Collections.Generic", "Dictionary", 2),
            new KnownTypeReference(KnownTypeCode.Void, "System", "Void"),
            new KnownTypeReference(KnownTypeCode.Array, "System", "Array"),
            new KnownTypeReference(KnownTypeCode.IntSeq, "Expresso.Runtime.Builtins", "ExpressoIntegerSequence"),
            new KnownTypeReference(KnownTypeCode.Slice, "Expresso.Runtime.Buitlins", "Slice", 2)
        };

        /// <summary>
        /// Gets the <see cref="KnownTypeReference"/> for the specified <see cref="KnownTypeCode"/>.
        /// Returns null for <c>KnownTypeCode.None</c>.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="typeCode">Type code.</param>
        public static KnownTypeReference Get(KnownTypeCode typeCode)
        {
            return knownTypeReferences[(int)typeCode];
        }

        readonly KnownTypeCode known_type_code;
        readonly string namespace_name;
        readonly string name;
        readonly int type_parameter_count;
        internal readonly CSharpKnownTypeCode baseType;

        public KnownTypeCode KnownTypeCode => known_type_code;
        public string Namespace => namespace_name;
        public string Name => name;
        internal int TypeParameterCount => type_parameter_count;

        KnownTypeReference(KnownTypeCode knownTypeCode, string namespaceName, string name, int typeParametercount = 0,
                           CSharpKnownTypeCode baseType = CSharpKnownTypeCode.Object)
        {
            known_type_code = knownTypeCode;
            namespace_name = namespaceName;
            this.name = name;
            type_parameter_count = typeParametercount;
            this.baseType = baseType;
        }

        public IType Resolve(ITypeResolveContext context)
        {
            return KnownTypeCache.FindType(context.Compilation, known_type_code);
        }

        public override string ToString()
        {
            return GetExpressoNameByTypeCode(known_type_code) ?? (Namespace + "." + Name);
        }

        /// <summary>
        /// Gets the Expresso primitive type name from the <see cref="KnownTypeCode"/>.
        /// Returns null if there is no primitive name for the specified type.
        /// </summary>
        /// <returns>The expresso name by type code.</returns>
        /// <param name="typeCode">Type code.</param>
        public static string GetExpressoNameByTypeCode(KnownTypeCode typeCode)
        {
            switch(typeCode){
            case KnownTypeCode.Bool:
                return "bool";

            case KnownTypeCode.Byte:
                return "byte";

            case KnownTypeCode.Char:
                return "char";

            case KnownTypeCode.Double:
                return "double";

            case KnownTypeCode.Float:
                return "float";

            case KnownTypeCode.Int:
                return "int";

            case KnownTypeCode.UInt:
                return "uint";

            case KnownTypeCode.Void:
                return "void";

            default:
                return null;
            }
        }
    }
}

