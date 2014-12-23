using System;
using Expresso.TypeSystem;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a primitive type.
    /// </summary>
    public class PrimitiveType : AstType
    {
        public static readonly TokenRole KeywordRole = new TokenRole("keyword");

        public ExpressoTokenNode KeywordToken{
            get{return GetChildByRole(KeywordRole);}
            set{SetChildByRole(KeywordRole, value);}
        }

        public KnownTypeCode KnownTypeCode{
            get{return GetKnownTypeCodeForPrimitiveType(KeywordToken);}
        }

        public PrimitiveType(string keyword, TextLocation location)
        {
            start_loc = location;
            end_loc = new TextLocation(location.Line, location.Column + keyword.Length);
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

        public static KnownTypeCode GetKnownTypeCodeForPrimitiveType(string keyword)
        {
            switch(keyword){
            case "Object":
                return KnownTypeCode.Object;

            case "Bool":
                return KnownTypeCode.Boolean;

            case "Byte":
                return KnownTypeCode.Byte;

            case "Int":
                return KnownTypeCode.Int;

            case "Float":
                return KnownTypeCode.Float;

            case "Bigint":
                return KnownTypeCode.BigInteger;

            case "Void":
                return KnownTypeCode.Void;
            }
        }
    }
}

