using System;
using Expresso.TypeSystem;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using Expresso.Ast.Analysis;

using ExpressoTypeCode = Expresso.TypeSystem.KnownTypeCode;
using ICSharpCode.NRefactory.TypeSystem.Implementation;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a primitive type reference node.
    /// In Expresso, primitive types are listed as follows:
    /// <list type="primitiveTypes">
    /// <item>int</item>
    /// <item>uint</item>
    /// <item>bool</item>
    /// <item>byte</item>
    /// <item>float</item>
    /// <item>double</item>
    /// <item>bigint</item>
    /// <item>char</item>
    /// <item>tuple</item>
    /// <item>vector</item>
    /// <item>dictionary</item>
    /// <item>function</item>
    /// <item>intseq</item>
    /// <item>slice</item>
    /// <item>array of the above types</item>
    /// </list>
    /// Note that the array, vector and dictionary types are builtin types and primitive types but we treat them as SimpleType nodes in AST.
    /// </summary>
    public class PrimitiveType : AstType
    {
        readonly string keyword;

        public ExpressoTypeCode KnownTypeCode => GetKnownTypeCodeForPrimitiveType(Keyword, this);

        public override string Name => keyword;

        /// <summary>
        /// Gets the keyword.
        /// </summary>
        /// <value>The keyword.</value>
        public string Keyword => keyword;

        public override Identifier IdentifierNode => Identifier.Null;

        public PrimitiveType(string keyword, TextLocation location)
            : base(location, new TextLocation(location.Line, location.Column + keyword.Length))
        {
            this.keyword = keyword;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitPrimitiveType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitPrimitiveType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitPrimitiveType(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            if(other is PlaceholderType)
                return true;
            
            var o = other as PrimitiveType;
            return o != null && Keyword == o.Keyword;
        }

        #region implemented abstract members of AstType
#if NETCOREAPP2_0
        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
        {
            var type_code = GetKnownTypeCodeForPrimitiveType(Keyword, null);
            if(type_code == ExpressoTypeCode.None)
                return new UnknownType(null, Keyword);
            else
                return Expresso.TypeSystem.KnownTypeReference.Get(type_code);
        }
#else
        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
        {
            throw new NotImplementedException();
        }
#endif

        #endregion

        /// <summary>
        /// Gets an <see cref="ExpressoTypeCode"/> instance corresponds to `keyword`.
        /// </summary>
        /// <returns>The known type code for primitive type.</returns>
        /// <param name="keyword">Keyword.</param>
        /// <param name="node">Node.</param>
        public static ExpressoTypeCode GetKnownTypeCodeForPrimitiveType(string keyword, AstNode node)
        {
            switch(keyword){
            case "bool":
                return ExpressoTypeCode.Bool;

            case "char":
                return ExpressoTypeCode.Char;

            case "byte":
                return ExpressoTypeCode.Byte;

            case "int":
                return ExpressoTypeCode.Int;

            case "uint":
                return ExpressoTypeCode.UInt;

            case "float":
                return ExpressoTypeCode.Float;
            
            case "double":
                return ExpressoTypeCode.Double;

            case "bigint":
                return ExpressoTypeCode.BigInteger;

            case "array":
                return ExpressoTypeCode.Array;

            case "vector":
                return ExpressoTypeCode.Vector;

            case "dictionary":
                return ExpressoTypeCode.Dictionary;

            case "tuple":
                return ExpressoTypeCode.Tuple;

            case "intseq":
                return ExpressoTypeCode.IntSeq;

            /*case "void":
                return ExpressoTypeCode.Void;*/

            case "string":
                return ExpressoTypeCode.String;

            case "slice":
                return ExpressoTypeCode.Slice;

            default:
                return ExpressoTypeCode.None;
            }
        }

        /// <summary>
        /// Gets a <see cref="ExpressoTypeCode"/> instance corresponds to the `keyword`.
        /// Note that it ignores the generic types.
        /// </summary>
        /// <returns>The actual known type code.</returns>
        /// <param name="keyword">Keyword.</param>
        /// <param name="node">Node.</param>
        public static ExpressoTypeCode GetActualKnownTypeCodeForPrimitiveType(string keyword, AstNode node)
        {
            switch(keyword){
            case "bool":
                return ExpressoTypeCode.Bool;

            case "char":
                return ExpressoTypeCode.Char;

            case "byte":
                return ExpressoTypeCode.Byte;

            case "int":
                return ExpressoTypeCode.Int;

            case "uint":
                return ExpressoTypeCode.UInt;

            case "float":
                return ExpressoTypeCode.Float;

            case "double":
                return ExpressoTypeCode.Double;

            case "bigint":
                return ExpressoTypeCode.BigInteger;

            case "intseq":
                return ExpressoTypeCode.IntSeq;

                /*case "void":
                return ExpressoTypeCode.Void;*/

            case "string":
                return ExpressoTypeCode.String;

            default:
                return ExpressoTypeCode.None;
            }
        }
    }
}

