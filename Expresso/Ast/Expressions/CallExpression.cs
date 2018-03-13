using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// 関数呼び出し。
	/// Represents a function, method or closure call. It binds an object when it references to a method
    /// otherwise it binds nothing.
    /// Expression '(' [ Arguments ] ')' ;
    /// </summary>
    public class CallExpression : Expression
    {
        /// <summary>
        /// 呼び出す対象。
		/// The target function to be called.
        /// </summary>
        public Expression Target{
            get{return GetChildByRole(Roles.TargetExpression);}
            set{SetChildByRole(Roles.TargetExpression, value);}
		}

        public ExpressoTokenNode LPar{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

        /// <summary>
        /// 与える実引数リスト。
		/// The argument list to be supplied to the call.
        /// </summary>
        public AstNodeCollection<Expression> Arguments{
            get{return GetChildrenByRole(Roles.Argument);}
		}

        public ExpressoTokenNode RPar{
            get{return GetChildByRole(Roles.RParenthesisToken);}
        }

        public CallExpression(Expression targetExpr, IEnumerable<Expression> arguments, TextLocation loc)
            : base(targetExpr.StartLocation, loc)
        {
            Target = targetExpr;
            Arguments.AddRange(arguments);
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitCallExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitCallExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitCallExpression(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as CallExpression;
            return o != null && Target.DoMatch(o.Target, match) && Arguments.DoMatch(o.Arguments, match);
        }

        #endregion
    }
}
