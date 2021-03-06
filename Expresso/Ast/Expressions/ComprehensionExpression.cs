using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
	/// <summary>
    /// Represents a list comprehension, which is a syntactic sugar for sequence initialization.
    /// Consider an expression, [x for x in 0..100],
    /// which is equivalent in functionality to the statement "for(let x in 0..100) yield x;"
    /// Expression "for" PatternConstruct "in" Expression { ComprehensionIter } ;
	/// </summary>
    public class ComprehensionExpression : Expression
	{
        public static readonly Role<ComprehensionForClause> ComprehensionBodyRole =
            new Role<ComprehensionForClause>("Body", ComprehensionForClause.Null);

        /// <summary>
        /// The expression that yields an item at a time.
        /// </summary>
		public Expression Item{
            get => GetChildByRole(Roles.Expression);
            set => SetChildByRole(Roles.Expression, value);
		}

        /// <summary>
        /// The root For clause.
        /// </summary>
		public ComprehensionForClause Body{
            get => GetChildByRole(ComprehensionBodyRole);
            set => SetChildByRole(ComprehensionBodyRole, value);
		}

        /// <summary>
        /// The type of the object to be produced.
        /// </summary>
        public SimpleType ObjectType{
            get => GetChildByRole(Roles.GenericType);
            set => SetChildByRole(Roles.GenericType, value);
		}

        public ComprehensionExpression(Expression itemExpr, ComprehensionForClause bodyExpr, SimpleType objType)
            : base(itemExpr.StartLocation, bodyExpr.EndLocation)
		{
            ObjectType = objType;
            Item = itemExpr;
            Body = bodyExpr;
		}

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitComprehensionExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitComprehensionExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitComprehensionExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ComprehensionExpression;
            return o != null && Item.DoMatch(o.Item, match) && Body.DoMatch(o.Body, match);
        }

        #endregion
	}

	public abstract class ComprehensionIter : Expression
	{
        #region Null
        public static new readonly ComprehensionIter Null = new NullComprehensionIter();

        sealed class NullComprehensionIter : ComprehensionIter
        {
            public override bool IsNull => true;

            public override void AcceptWalker(IAstWalker walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data) => walker.VisitNullNode(this, data);

            internal protected override bool DoMatch(AstNode other, Match match) => other == null || other.IsNull;
        }
        #endregion

        public static readonly Role<ComprehensionIter> CompBody = new Role<ComprehensionIter>("CompBody", Null);

        protected ComprehensionIter()
        {
        }

        protected ComprehensionIter(TextLocation start, TextLocation end)
            : base(start, end)
        {
        }
	}

    /// <summary>
    /// Represents a for clause in comprehension expressions.
    /// "for" PatternConstruct "in" Expression [ ComprehensionIter ] ;
    /// </summary>
    public class ComprehensionForClause : ComprehensionIter
	{
        #region Null
        public static new readonly ComprehensionForClause Null = new NullComprehensionForClause();

        sealed class NullComprehensionForClause : ComprehensionForClause
        {
            public override bool IsNull => true;

            public override void AcceptWalker(IAstWalker walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data) => walker.VisitNullNode(this, data);

            internal protected override bool DoMatch(AstNode other, Match match) => other == null || other.IsNull;
        }
        #endregion

		/// <summary>
        /// body内で操作対象となるオブジェクトを参照するのに使用する式。
        /// 評価結果はlvalueにならなければならない。
        /// なお、走査の対象を捕捉する際には普通の代入と同じルールが適用される。
        /// つまり、複数の変数にいっせいにオブジェクトを捕捉させることもできる。
        /// When evaluating the both sides of the "in" keyword,
        /// the same rule as the assignment applies.
        /// So for example,
        /// for(x, y in [1,2,3,4,5,6])...
        /// the x and y captures the first and second element of the list at the first time,
        /// the third and forth the next time, and the fifth and sixth the last time.
        /// </summary>
        public PatternConstruct Left{
            get => GetChildByRole(Roles.Pattern);
            set => SetChildByRole(Roles.Pattern, value);
		}

        /// <summary>
        /// 走査する対象の式。
        /// The target expression.
        /// </summary>
        public Expression Target{
            get => GetChildByRole(Roles.TargetExpression);
            set => SetChildByRole(Roles.TargetExpression, value);
		}

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{
            get => GetChildByRole(CompBody);
            set => SetChildByRole(CompBody, value);
		}

        protected ComprehensionForClause()
        {
        }

        public ComprehensionForClause(PatternConstruct lhs, Expression targetExpr, ComprehensionIter bodyExpr)
            : base(lhs.StartLocation, bodyExpr == null ? targetExpr.EndLocation : bodyExpr.EndLocation)
		{
            Left = lhs;
            Target = targetExpr;
            Body = bodyExpr;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitComprehensionForClause(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitComprehensionForClause(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitComprehensionForClause(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ComprehensionForClause;
            return o != null && Left.DoMatch(o.Left, match) && Target.DoMatch(o.Target, match)
                && Body.DoMatch(o.Body, match);
        }

        #endregion
	}

    /// <summary>
    /// Represents an if clause in comprehension expressions.
    /// "if" Expression [ ComprehensionIter ] ;
    /// </summary>
    public class ComprehensionIfClause : ComprehensionIter
	{
		/// <summary>
        /// 実行対象の文。
        /// The condition to be tested in order to determine whether to execute the body expression.
        /// </summary>
        public Expression Condition{
            get => GetChildByRole(Roles.Expression);
            set => SetChildByRole(Roles.Expression, value);
		}

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{
            get => GetChildByRole(CompBody);
            set => SetChildByRole(CompBody, value);
		}

        public ComprehensionIfClause(Expression test, ComprehensionIter bodyIter)
            : base(test.StartLocation, (bodyIter != null) ? bodyIter.EndLocation : test.EndLocation)
		{
            Condition = test;
            Body = bodyIter;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitComprehensionIfClause(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitComprehensionIfClause(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitComprehensionIfClause(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ComprehensionIfClause;
            return o != null && Condition.DoMatch(o.Condition, match) && Body.DoMatch(o.Body, match);
        }

        #endregion
	}
}

