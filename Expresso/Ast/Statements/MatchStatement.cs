using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Compiler;
using Expresso.Runtime;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/// <summary>
    /// Match文。
    /// The Match statement.
    /// "match" Expression '{' MatchPatternClause { MatchPatternClause } '}'
	/// </summary>
    public class MatchStatement : Statement
	{
        public static readonly TokenRole MatchKeywordRole = new TokenRole("match");
        public static readonly Role<Expression> PatternClauseRole = new Role<Expression>("PatternClause", Expression.Null);

        public ExpressoTokenNode MatchToken{
            get{GetChildByRole(MatchKeywordRole);}
        }

		/// <summary>
        /// 評価対象となる式。
		/// The target expression on which we'll choose which path to go.
        /// </summary>
        public Expression Target{
            get{return GetChildByRole(Roles.TargetExpression);}
		}

        /// <summary>
        /// 分岐先となるパターン(郡)。
        /// </summary>
        public AstNodeCollection<MatchPatternClause> Clauses{
            get{return GetChildrenByRole(PatternClauseRole);}
		}

        public MatchStatement(Expression targetExpr, IEnumerable<MatchPatternClause> patternClauses)
		{
            AddChild(targetExpr, Roles.TargetExpression);
            foreach(var pattern_clause in patternClauses)
                AddChild(pattern_clause, PatternClauseRole);
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitSwitchStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSwitchStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitMatchStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as MatchStatement;
            return o != null && Target.DoMatch(o.Target, match) && Clauses.DoMatch(o.Clauses, match);
        }

        #endregion
	}

	/// <summary>
    /// Represents a pattern clause in match statements.
    /// PatternConstruct { '|' PatternConstruct } "=>" Statement
	/// </summary>
	public class MatchPatternClause : Expression
	{
		/// <summary>
        /// 分岐先となるパターン(郡)。
        /// </summary>
        public AstNodeCollection<PatternConstruct> Labels{
            get{return GetChildrenByRole(Roles.Pattern);}
		}

        /// <summary>
        /// 実行対象の文(ブロック)。
		/// The body statement or block.
        /// </summary>
        public Statement Body{
            get{return GetChildByRole(Roles.Body);}
		}

        public MatchPatternClause(IEnumerable<PatternConstruct> patternExprs, Statement bodyStmt)
		{
            foreach(var label in patternExprs)
                AddChild(label, Roles.Expression);

            AddChild(bodyStmt, Roles.Body);
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitCaseClause(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitCaseClause(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitCaseClause(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as MatchPatternClause;
            return o != null && Labels.DoMatch(o.Labels, match) && Body.DoMatch(o.Body, match);
        }
	}
}

