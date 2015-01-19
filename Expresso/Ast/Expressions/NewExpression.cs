using System;

using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
    /// <summary>
    /// ヒープオブジェクト生成式。
	/// Reperesents a new expression.
    /// A new expression always allocate memory on the heap.
    /// "new" ObjectCreationExpression ;
    /// </summary>
    public class NewExpression : Expression
    {
        public static readonly Role<ObjectCreationExpression> ObjectCreationRole =
            new Role<ObjectCreationExpression>("ObjectCreation");

        /// <summary>
        /// オブジェクトを生成するクラスの定義を参照する式。
		/// The target class definition of the new expression.
        /// </summary>
        public ObjectCreationExpression CreationExpression{
            get{return GetChildByRole(ObjectCreationRole);}
            set{SetChildByRole(ObjectCreationRole, value);}
		}

        public NewExpression(ObjectCreationExpression objectCreation)
		{
            CreationExpression = objectCreation;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitNewExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitNewExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitNewExpression(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as NewExpression;
            return o != null && CreationExpression.DoMatch(o.CreationExpression, match);
        }

        #endregion
    }
}
