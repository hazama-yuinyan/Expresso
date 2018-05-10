using System;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
	/// <summary>
	/// For文。
	/// The For statement.
    /// A for statement is used for iterating over sequence-like objects.
    /// "for" PatternConstruct "in" Expression '{' Body '}' ;
	/// </summary>
    public class ForStatement : Statement
	{
        public static readonly Role<Expression> IterableRole = new Role<Expression>("Iterable", Expression.Null);

        public ExpressoTokenNode ForToken => GetChildByRole(Roles.ForToken);

		/// <summary>
        /// body内で操作対象となるオブジェクトを参照するのに使用する式。
        /// 評価結果はlvalueにならなければならない。
        /// なお、走査の対象を捕捉する際には普通の代入と同じルールが適用される。
        /// つまり、複数の変数にいっせいにオブジェクトを捕捉させることもできる。
        /// When evaluating the both sides of the "in" keyword,
        /// the same rule as the assignment applies.
        /// So for example,
        /// for(let (x, y) in [1,2,3,4,5,6])...
        /// the x and y captures the first and second element of the list at the first time,
        /// the third and forth the next time, and the fifth and sixth the last time.
        /// </summary>
        public PatternConstruct Left{
            get{return GetChildByRole(Roles.Pattern);}
            set{SetChildByRole(Roles.Pattern, value);}
        }

        /// <summary>
        /// 走査する対象の式。
        /// The target expression to be iterated over. It must yield an iterable object, otherwise a compile-time error
        /// (when compiling the code) or a runtime exception occurs(when in interpreter mode)
        /// </summary>
        public Expression Target{
            get{return GetChildByRole(IterableRole);}
            set{SetChildByRole(IterableRole, value);}
        }

        /// <summary>
        /// 操作対象のオブジェクトが存在する間評価し続けるブロック。
        /// The block we'll continue to evaluate until the sequence is ate up.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }

        public ForStatement(PatternConstruct left, Expression iterable, BlockStatement body,
            TextLocation loc)
            : base(loc, body.EndLocation)
        {
            Left = left;
            Target = iterable;
            Body = body;
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

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ForStatement;
            return o != null && Left.DoMatch(o.Left, match)
                && Target.DoMatch(o.Target, match) && Body.DoMatch(o.Body, match);
        }

        #endregion
	}
}

