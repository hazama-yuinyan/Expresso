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
    /// </summary>
    public class PrimitiveType : AstType
    {
        public static readonly TokenRole KeywordRole = new TokenRole("keyword");

        public ExpressoTokenNode KeywordToken{
            get{return GetChildByRole(KeywordRole);}
            set{SetChildByRole(KeywordRole, value);}
        }

        public ExpressoTypeCode KnownTypeCode{
            get{return GetKnownTypeCodeForPrimitiveType(TokenRole.Tokens[(int)KeywordToken.RoleIndex >> (int)AstNodeFlagsUsedBits]);}
        }

        public PrimitiveType(string keyword, TextLocation location)
            : base(location, new TextLocation(location.Line, location.Column + keyword.Length))
        {
            KeywordToken = new ExpressoTokenNode(location, new TokenRole(keyword));
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
            return o != null && o.KeywordToken == KeywordToken;
        }

        #region implemented abstract members of AstType

        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        public static ExpressoTypeCode GetKnownTypeCodeForPrimitiveType(string keyword)
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

            case "void":
                return ExpressoTypeCode.Void;

            default:
                throw new ParserException("{0} is an unknown primitive type!", keyword);
            }
        }
    }
}

