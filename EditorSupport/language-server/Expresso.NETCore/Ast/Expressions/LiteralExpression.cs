using System;

using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
    /// <summary>
    /// 定数。
    /// Represents a literal expression.
    /// A literal value is evaluated to written words or numbers.
    /// </summary>
    public class LiteralExpression : Expression
    {
        #region Null
        public static new readonly LiteralExpression Null = new NullLiteralExpression();

        sealed class NullLiteralExpression : LiteralExpression
        {
            public override bool IsNull{
                get{
                    return true;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitNullNode(this, data);
            }

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        public static readonly object AnyValue = new object();

        object value;
        string literal_value;

		/// <summary>
		/// この定数値の型。
		/// The type of this constant.
		/// </summary>
        public AstType Type{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
		}
		
        /// <summary>
        /// 定数の値。
		/// The value.
        /// </summary>
        public object Value{
            get{return value;}
            set{
                ThrowIfFrozen();
                this.value = value;
                literal_value = null;
            }
		}

        public string LiteralValue{
            get{return literal_value ?? "";}
        }

        protected LiteralExpression()
        {
        }

        public LiteralExpression(object value, AstType valType, TextLocation loc)
            : base(loc, new TextLocation(loc.Line, loc.Column + value.ToString().Length))
		{
            Type = valType;
            Value = value;
            literal_value = value.ToString();
		}

        public void SetValue(object value, string literalValue)
        {
            if(value == null)
                throw new ArgumentNullException(nameof(value));

            Value = value;
            literal_value = literalValue;
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitLiteralExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitLiteralExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitLiteralExpression(this, data);
        }

        unsafe static TextLocation AdvanceLocation(TextLocation startLocation, string str)
        {
            int line = startLocation.Line;
            int col  = startLocation.Column;
            fixed(char* start = str){
                char* p = start;
                char* endPtr = start + str.Length;
                while(p < endPtr){
                    var nl = NewLine.GetDelimiterLength(*p, () => {
                        char* nextp = p + 1;
                        if(nextp < endPtr)
                            return *nextp;

                        return '\0';
                    });
                    if(nl > 0){
                        line++;
                        col = 1;
                        if(nl == 2)
                            p++;
                    }else{
                        col++;
                    }
                    p++;
                }
            }
            return new TextLocation(line, col);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as LiteralExpression;
            return o != null && (this.value == AnyValue || object.Equals(this.value, o.value));
        }

        #endregion
    }
}
