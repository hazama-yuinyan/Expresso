using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


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
        public static readonly Role<FunctionType> OverloadSignatureRole = new Role<FunctionType>("OverloadSignature", FunctionType.Null);

        /// <summary>
        /// 呼び出す対象。
		/// The target function to be called.
        /// </summary>
        public Expression Target{
            get => GetChildByRole(Roles.TargetExpression);
            set => SetChildByRole(Roles.TargetExpression, value);
		}

        public ExpressoTokenNode LPar => GetChildByRole(Roles.LParenthesisToken);

        /// <summary>
        /// 実型引数リスト。
        /// Represents the type arguments.
        /// </summary>
        /// <value>The type arguments.</value>
        public AstNodeCollection<AstType> TypeArguments => GetChildrenByRole(Roles.TypeArgument);

        /// <summary>
        /// 与える実引数リスト。
		/// The argument list to be supplied to the call.
        /// </summary>
        public AstNodeCollection<Expression> Arguments => GetChildrenByRole(Roles.Argument);

        /// <summary>
        /// Represents the signature to call.
        /// Used to resolve the method overload.
        /// </summary>
        /// <value>The overload signature.</value>
        public FunctionType OverloadSignature{
            get => GetChildByRole(OverloadSignatureRole);
            set => SetChildByRole(OverloadSignatureRole, value);
        }

        public ExpressoTokenNode RPar => GetChildByRole(Roles.RParenthesisToken);

        public CallExpression(Expression targetExpr, IEnumerable<AstType> typeArgs, IEnumerable<Expression> arguments, TextLocation loc)
            : base(targetExpr.StartLocation, loc)
        {
            Target = targetExpr;
            if(typeArgs != null)
                TypeArguments.AddRange(typeArgs);

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

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as CallExpression;
            return o != null && Target.DoMatch(o.Target, match) && Arguments.DoMatch(o.Arguments, match);
        }

        #endregion
    }
}
