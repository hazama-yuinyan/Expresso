using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Runtime;
using Expresso.Compiler;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/// <summary>
	/// Try文。
	/// The Try statement.
	/// </summary>
    public class TryStatement : Statement
	{
        public static readonly Role<Expression> CatchClauseRole = new Role<Expression>("CatchClause", Expression.Null);
        public static readonly Role<Expression> FinallyClauseRole = new Role<Expression>("FinallyClause", Expression.Null);

		/// <summary>
        /// 例外の捕捉を行うブロック。
		/// The block in which we'll catch exceptions.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body);}
		}

        /// <summary>
        /// Catch節。
		/// Represents the catch clause of this try statement.
		/// It can be null if none is specified.
        /// </summary>
        public AstNodeCollection<CatchClause> Catches{
            get{return GetChildrenByRole(CatchClauseRole);}
		}

		/// <summary>
		/// Finally節。
		/// Represents the finally clause of this try statement.
		/// It can be null if none is specified.
		/// </summary>
		public FinallyClause FinallyClause{
            get{return GetChildByRole(FinallyClauseRole);}
            set{SetChildByRole(FinallyClauseRole);}
		}

        public TryStatement(BlockStatement bodyBlock, IEnumerable<CatchClause> catches, FinallyClause finallyClause)
		{
            Body = bodyBlock;
            FinallyClause = finallyClause;
            foreach(var catch_clause in catches)
                AddChild(catch_clause, CatchClauseRole);
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitTryStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitTryStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitTryStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as TryStatement;
            return o != null && Catches.DoMatch(o.Catches, match) && FinallyClause.DoMatch(o.FinallyClause, match)
                && Body.DoMatch(o.Body, match);
        }

        #endregion
	}

	/// <summary>
	/// Represents a catch clause in a try statement.
	/// </summary>
    public class CatchClause : Expression
	{
        public static readonly TokenRole CatchKeywordRole = new TokenRole("catch");

        public ExpressoTokenNode CatchToken{
            get{return GetChildByRole(CatchKeywordRole);}
        }

        public ExpressoTokenNode LPar{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

		/// <summary>
        /// catchの対象となる例外の型とその識別子名。
		/// The target type and the name which this node will catch.
        /// </summary>
        public Identifier CatcherToken{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
		}

        public string Catcher{
            get{return GetChildByRole(Roles.Identifier).Name;}
        }

        public ExpressoTokenNode RPar{
            get{return GetChildByRole(Roles.RParenthesisToken);}
        }

        /// <summary>
        /// 実行対象のブロック。
		/// The body block.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
		}

        public override NodeType NodeType{
            get{return NodeType.Expression;}
        }

		public CatchClause(Identifier catcher, BlockStatement bodyBlock)
		{
            CatcherToken = catcher;
            Body = bodyBlock;
		}

        public override void AcceptWalker(IAstWalker walker)
        {
            return walker.VisitCatchClause(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitCatchClause(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitCatchClause(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as CatchClause;
            return CatcherToken.DoMatch(o.CatcherToken, match) && Body.DoMatch(o.Body, match);
        }

        #endregion
	}

	/// <summary>
	/// Represents a finally clause in a try statement.
	/// </summary>
    public class FinallyClause : Expression
	{
        /// <summary>
        /// 実行対象のブロック。
		/// The body block.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
		}

        public override NodeType NodeType{
            get{return NodeType.Expression;}
        }

		public FinallyClause(BlockStatement bodyBlock)
		{
            AddChild(bodyBlock, Roles.Body);
		}

        public override void AcceptWalker(IAstWalker walker)
        {
            return walker.VisitFinallyClause(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitFinallyClause(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitFinallyClause(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as FinallyClause;
            return o != null && Body.DoMatch(o.Body, match);
        }

        #endregion
	}
}

