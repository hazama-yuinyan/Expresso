using System;


namespace Expresso.Ast
{
    /// <summary>
    /// キャスト式。
	/// The cast expression.
    /// A cast expression casts a value to another type.
    /// In Expresso, since there are no null values, any invalid type casts must and have to be
    /// a compilation error.
    /// Expression "as" Type ;
    /// </summary>
    public class CastExpression : Expression
    {
        /// <summary>
        /// キャスト先の型。
		/// The target type to which the expression casts the object.
        /// </summary>
        public AstType ToExpression{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
		}

        /// <summary>
        /// キャストを実行するオブジェクト。
		/// The target object to be casted.
        /// </summary>
        public Expression Target{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
		}

        public CastExpression(AstType toExpr, Expression targetExpr)
		{
            Target = targetExpr;
            ToExpression = toExpr;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitCastExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitCastExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitCastExpression(this, data);
        }

        #region implemented abstract members of AstNode
        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as CastExpression;
            return o != null && Target.DoMatch(o.Target, match) && ToExpression.DoMatch(o.ToExpression, match);
        }
        #endregion
    }
}
