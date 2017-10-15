using System;
using Expresso.TypeSystem;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using Expresso.Ast.Analysis;

using ExpressoTypeCode = Expresso.TypeSystem.KnownTypeCode;

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
    /// <item>array of the above types</item>
    /// </list>
    /// </summary>
    public class PrimitiveType : AstType
    {
        string keyword;

        public ExpressoTypeCode KnownTypeCode{
            get{return GetKnownTypeCodeForPrimitiveType(Keyword, this);}
        }

        public override string Name{
            get{return keyword;}
        }

        public string Keyword{
            get{return keyword;}
            set{
                if(value == null)
                    throw new ArgumentNullException();

                ThrowIfFrozen();
                keyword = value;
            }
        }

        public override Identifier IdentifierNode{
            get{return null;}
        }

        public PrimitiveType(string keyword, TextLocation location)
            : base(location, new TextLocation(location.Line, location.Column + keyword.Length))
        {
            Keyword = keyword;
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
            var o = other as PrimitiveType;
            return o != null && Keyword == o.Keyword;
        }

        #region implemented abstract members of AstType

        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        public static ExpressoTypeCode GetKnownTypeCodeForPrimitiveType(string keyword, AstNode node)
        {
            switch(keyword){
            case "array":
                return ExpressoTypeCode.Array;

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

            case "vector":
                return ExpressoTypeCode.Vector;

            case "dictionary":
                return ExpressoTypeCode.Dictionary;

            case "function":
                return ExpressoTypeCode.Function;

            case "tuple":
                return ExpressoTypeCode.Tuple;

            case "intseq":
                return ExpressoTypeCode.IntSeq;

            /*case "void":
                return ExpressoTypeCode.Void;*/

            case "string":
                return ExpressoTypeCode.String;

            default:
                throw new ParserException("{0} is an unknown primitive type!", node, keyword);
            }
        }
    }
}

