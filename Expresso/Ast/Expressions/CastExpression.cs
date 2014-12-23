using System.Collections.Generic;
using System.Linq;

using Expresso.Compiler;

namespace Expresso.Ast
{
    /// <summary>
    /// キャスト式。
	/// The cast expression.
    /// </summary>
    public class CastExpression : Expression
    {
        /// <summary>
        /// キャスト先の型。
		/// The target type to which the expression casts the object.
        /// </summary>
        public Expression ToExpression{
            get{return FirstChild;}
		}

        /// <summary>
        /// キャストを実行するオブジェクト。
		/// The target object to be casted.
        /// </summary>
        public Expression Target{
            get{return LastChild;}
		}

        public override NodeType NodeType{
            get{return NodeType.CastExpression;}
        }

		public CastExpression(Expression toExpr, Expression targetExpr)
		{
            AddChild(toExpr);
            AddChild(targetExpr);
		}

        public override bool Equals(object obj)
        {
            var x = obj as CastExpression;

            if(x == null)
                return false;

            return this.ToExpression == x.ToExpression && this.Target.Equals(x.Target);
        }

        public override int GetHashCode()
        {
            return this.ToExpression.GetHashCode() ^ this.Target.GetHashCode();
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

        public override string GetText()
		{
            return string.Format("<Cast: {0} => {1}>", Target.GetText(), ToExpression.GetText());
		}

        public override string ToString()
        {
            return GetText();
        }
    }
}
