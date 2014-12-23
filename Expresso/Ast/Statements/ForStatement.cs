using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Expresso.Runtime;
using Expresso.Compiler;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/**
	 * For文はインスタンス生成時に全てのメンバーが決定しないので、メンバーは全部変更可能にしておく。
	 */

	/// <summary>
	/// For文。
	/// The For statement.
	/// </summary>
	/// <seealso cref="BreakableStatement"/>
    public class ForStatement : Statement
	{
        public static readonly Role<Expression> InitializerRole = new Role<Expression>("Initializer", Expression.Null);
        public static readonly Role<Expression> IterableRole = new Role<Expression>("Iterable", Expression.Null);

        public ExpressoTokenNode ForToken{
            get{return GetChildByRole(Roles.ForToken);}
        }

        public ExpressoTokenNode LPar{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

		/// <summary>
        /// body内で操作対象となるオブジェクトを参照するのに使用する式。
        /// 評価結果はlvalueにならなければならない。
        /// なお、走査の対象を捕捉する際には普通の代入と同じルールが適用される。
        /// つまり、複数の変数にいっせいにオブジェクトを捕捉させることもできる。
        /// When evaluating the both sides of the "in" keyword,
        /// the same rule as the assignment applies.
        /// So for example,
        /// for(let x, y in [1,2,3,4,5,6])...
        /// the x and y captures the first and second element of the list at the first time,
        /// the third and forth the next time, and the fifth and sixth at last.
        /// </summary>
		public Expression Left{
            get{return GetChildByRole(InitializerRole);}
        }

        /// <summary>
        /// 走査する対象の式。
        /// The target expression to be iterated over. It must yields a iterable object, otherwise a compile-time error
        /// (when compiling the code) or a runtime exception occurs(when in interpreter mode)
        /// </summary>
        public Expression Target{
            get{return GetChildByRole(IterableRole);}
        }

        public ExpressoTokenNode RPar{
            get{return GetChildByRole(Roles.RParenthesisToken);}
        }

        /// <summary>
        /// 操作対象のオブジェクトが存在する間評価し続けるブロック。
        /// The block we'll continue to evaluate until the sequence is ate up.
        /// </summary>
        public Statement Body{
            get{return GetChildByRole(Roles.Body);}
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitForStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitForStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitForStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ForStatement;
            return o != null && Left.DoMatch(o.Left, match)
                && Target.DoMatch(o.Target, match) && Body.DoMatch(o.Body, match);
        }

        #endregion
	}
}

