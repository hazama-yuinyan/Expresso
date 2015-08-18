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
        public static readonly Role<ExpressoTokenNode> KeywordRole =
            new Role<ExpressoTokenNode>("Keyword", ExpressoTokenNode.Null);

        public ExpressoTokenNode KeywordToken{
            get{return GetChildByRole(KeywordRole);}
            set{SetChildByRole(KeywordRole, value);}
        }

        public ExpressoTypeCode KnownTypeCode{
            get{return GetKnownTypeCodeForPrimitiveType(KeywordToken.Token);}
        }

        public override string Name{
            get{return KeywordToken.Token;}
        }

        public override Identifier IdentifierNode{
            get{return GetChildByRole(Roles.Identifier);}
        }

        public PrimitiveType(string keyword, TextLocation location)
            : base(location, new TextLocation(location.Line, location.Column + keyword.Length))
        {
            KeywordToken = new ExpressoTokenNode(location, new TokenRole(keyword, ExpressoTokenNode.Null));
            SetChildByRole(Roles.Identifier, AstNode.MakeIdentifier(keyword));
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
            return o != null && KeywordToken.DoMatch(o.KeywordToken, match);
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

            case "void":
                return ExpressoTypeCode.Void;

            case "string":
                return ExpressoTypeCode.String;

            default:
                throw new ParserException("{0} is an unknown primitive type!", keyword);
            }
        }
    }
}

