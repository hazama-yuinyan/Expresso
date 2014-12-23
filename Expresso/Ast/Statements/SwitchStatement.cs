using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Compiler;
using Expresso.Runtime;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/// <summary>
	/// Switch文。
	/// The Switch statement.
	/// </summary>
    public class SwitchStatement : Statement
	{
        public static readonly TokenRole SwitchKeywordRole = new TokenRole("switch");
        public static readonly Role<Expression> CaseClauseRole = new Role<Expression>("CaseClause", Expression.Null);

        public ExpressoTokenNode SwitchToken{
            get{GetChildByRole(SwitchKeywordRole);}
        }

        public ExpressoTokenNode LPar{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

		/// <summary>
        /// 評価対象となる式。
		/// The target expression on which we'll choose which path to go.
        /// </summary>
        public Expression Target{
            get{return GetChildByRole(Roles.TargetExpression);}
		}

        public ExpressoTokenNode RPar{
            get{return GetChildByRole(Roles.RParenthesisToken);}
        }

        /// <summary>
        /// 分岐先となるラベル(郡)。
        /// </summary>
        public AstNodeCollection<CaseClause> Cases{
            get{return GetChildrenByRole(CaseClauseRole);}
		}

        public SwitchStatement(Expression targetExpr, IEnumerable<CaseClause> caseClauses)
		{
            AddChild(targetExpr, Roles.TargetExpression);
            foreach(var case_clause in caseClauses)
                AddChild(case_clause, CaseClauseRole);
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
            return walker.VisitSwitchStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as SwitchStatement;
            return o != null && Target.DoMatch(o.Target, match) && Cases.DoMatch(o.Cases, match);
        }

        #endregion
	}

	/// <summary>
	/// Represents a case clause in switch statements.
	/// </summary>
	public class CaseClause : Expression
	{
        public static readonly TokenRole CaseKeywordRole = new TokenRole("case");
        public static readonly TokenRole DefaultKeywordRole = new TokenRole("default");

        public ExpressoTokenNode CaseToken{
            get{return GetChildByRole(CaseKeywordRole);}
        }

		/// <summary>
        /// 分岐先となるラベル(郡)。
        /// </summary>
        public AstNodeCollection<Expression> Labels{
            get{return GetChildrenByRole(Roles);}
		}

        /// <summary>
        /// 実行対象の文(ブロック)。
		/// The body statement or block.
        /// </summary>
        public Statement Body{
            get{return GetChildByRole(Roles.Body);}
		}

        public CaseClause(IEnumerable<Expression> labelExprs, Statement bodyStmt)
		{
            foreach(var label in labelExprs)
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
            var o = other as CaseClause;
            return o != null && Labels.DoMatch(o.Labels, match) && Body.DoMatch(o.Body, match);
        }
	}
}

