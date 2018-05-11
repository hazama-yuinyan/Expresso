using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
	/// <summary>
    /// Match文。
    /// The Match statement.
    /// A match statement pattern-matches a value against several patterns
    /// and if successful the flow takes the corresponding branch and continues execution on that path.
    /// "match" Expression '{' MatchPatternClause { MatchPatternClause } '}' ;
	/// </summary>
    public class MatchStatement : Statement
	{
        public static readonly TokenRole MatchKeywordRole = new TokenRole("match", ExpressoTokenNode.Null);
        public static readonly Role<MatchPatternClause> PatternClauseRole = new Role<MatchPatternClause>("PatternClause");

        public ExpressoTokenNode MatchToken => GetChildByRole(MatchKeywordRole);

		/// <summary>
        /// 評価対象となる式。
		/// The target expression on which we'll choose which path to go.
        /// </summary>
        public Expression Target{
            get => GetChildByRole(Roles.TargetExpression);
            set => SetChildByRole(Roles.TargetExpression, value);
		}

        /// <summary>
        /// 分岐先となるパターン(郡)。
        /// Branches with patterns.
        /// </summary>
        public AstNodeCollection<MatchPatternClause> Clauses => GetChildrenByRole(PatternClauseRole);

        public MatchStatement(Expression targetExpr, IEnumerable<MatchPatternClause> patternClauses,
            TextLocation start, TextLocation end)
            : base(start, end)
		{
            Target = targetExpr;
            Clauses.AddRange(patternClauses);
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitMatchStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitMatchStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitMatchStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as MatchStatement;
            return o != null && Target.DoMatch(o.Target, match) && Clauses.DoMatch(o.Clauses, match);
        }

        #endregion
	}

	/// <summary>
    /// Represents a pattern clause in match statements.
    /// A pattern clause represents a clause that contains patterns that should be matched and the corresponding code
    /// that will be exceuted if the patterns match the value.
    /// PatternConstruct { '|' PatternConstruct } "=>" Statement
	/// </summary>
	public class MatchPatternClause : Expression
	{
		/// <summary>
        /// 分岐条件となるパターン(郡)。
        /// Patterns used to determine whether the flow takes the corresponding branch or not.
        /// </summary>
        public AstNodeCollection<PatternConstruct> Patterns => GetChildrenByRole(Roles.Pattern);

        /// <summary>
        /// ガード式。
        /// The guard expression. A guard expression is the additional condition that has to be satisfied
        /// so that the path will be taken.
        /// </summary>
        public Expression Guard{
            get => GetChildByRole(Roles.Expression);
            set => SetChildByRole(Roles.Expression, value);
        }

        /// <summary>
        /// 実行対象の文(ブロック)。
		/// The body statement or block.
        /// </summary>
        public Statement Body{
            get => GetChildByRole(Roles.EmbeddedStatement);
            set => SetChildByRole(Roles.EmbeddedStatement, value);
		}

        public MatchPatternClause(IEnumerable<PatternConstruct> patternExprs, Expression guard, Statement bodyStmt)
            : base(patternExprs.First().StartLocation, bodyStmt.EndLocation)
		{
            Patterns.AddRange(patternExprs);
            Guard = guard;
            Body = bodyStmt;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitMatchClause(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitMatchClause(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitMatchClause(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as MatchPatternClause;
            return o != null && Patterns.DoMatch(o.Patterns, match) && Guard.DoMatch(o.Guard, match) && Body.DoMatch(o.Body, match);
        }
	}
}
