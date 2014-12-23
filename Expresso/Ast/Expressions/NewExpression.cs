using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Compiler;

namespace Expresso.Ast
{
    /// <summary>
    /// オブジェクト生成式。
	/// Reperesents a new expression.
    /// </summary>
    public class NewExpression : Expression
    {
        /// <summary>
        /// オブジェクトを生成するクラスの定義を参照する式。
		/// The target class definition of the new expression.
        /// </summary>
        public Expression TargetExpr{
            get{return GetChildByRole(Roles.TargetExpression);}
		}

		/// <summary>
		/// コンストラクタに渡す引数。
		/// The argument list that will be passed to the constructor.
		/// </summary>
        public AstNodeCollection<Expression> Arguments{
            get{return GetChildrenByRole(Roles.Argument);}
		}

		public NewExpression(Expression targetExpr, Expression[] arguments)
		{
            AddChild(targetExpr, Roles.TargetExpression);
            foreach(var arg in arguments)
                AddChild(arg, Roles.Argument);
		}

        public override bool Equals(object obj)
        {
            var x = obj as NewExpression;

            if(x == null)
                return false;

            return this.TargetExpr == x.TargetExpr;
        }

        public override int GetHashCode()
        {
            return this.TargetExpr.GetHashCode();
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
    }
}
