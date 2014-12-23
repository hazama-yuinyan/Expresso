using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Compiler.Meta;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
	/// <summary>
	/// Represents a list comprehension, which is syntactic sugar for sequence initialization.
    /// Consider an expression, [x for x in [0..100]],
    /// which is equivalent in functionality to the statement "for(let x in [0..100]) yield x;"
	/// </summary>
    public class ComprehensionExpression : Expression
	{
        public static readonly Role<ComprehensionForClause> CompBodyRole = new Role<ComprehensionForClause>("Body", ComprehensionForClause.Null);

        /// <summary>
        /// The expression that yields an item at a time.
        /// </summary>
        /// <value>The item.</value>
		public Expression Item{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
		}

        /// <summary>
        /// The root for clause.
        /// </summary>
        /// <value>The body.</value>
		public ComprehensionForClause Body{
            get{return GetChildByRole(CompBodyRole);}
            set{SetChildByRole(CompBodyRole, value);}
		}

        /// <summary>
        /// The type of the object to be produced.
        /// </summary>
        /// <value>The type of the object.</value>
        public AstType ObjectType{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
		}

        public override NodeType NodeType{
            get{return NodeType.Expression;}
        }

        public ComprehensionExpression(Expression itemExpr, ComprehensionForClause bodyExpr, AstType objType)
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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ComprehensionExpression;
            return o != null && Item.DoMatch(o.Item, match) && Body.DoMatch(o.Body, match);
        }

        #endregion
	}

	public abstract class ComprehensionIter : Expression
	{
        public static readonly Role<ComprehensionIter> CompBody = new Role<ComprehensionIter>("CompBody", ComprehensionIter.Null);
	}

    /// <summary>
    /// Represents the for clause in a comprehension expression.
    /// </summary>
    public class ComprehensionForClause : ComprehensionIter
	{
        public static readonly Role<SequenceExpression> References = new Role<SequenceExpression>("References", SequenceExpression.Null);

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
        public SequenceExpression Left{
            get{return GetChildByRole(References);}
            set{SetChildByRole(References, value);}
		}

        /// <summary>
        /// 走査する対象の式。
        /// The target expression.
        /// </summary>
        public Expression Target{
            get{return GetChildByRole(Roles.TargetExpression);}
            set{SetChildByRole(Roles.TargetExpression, value);}
		}

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{
            get{return GetChildByRole(ComprehensionIter.CompBody);}
            set{SetChildByRole(ComprehensionIter.CompBody, value);}
		}

        public override NodeType NodeType{
            get{return NodeType.Expression;}
        }

		public ComprehensionForClause(SequenceExpression lhs, Expression targetExpr, ComprehensionIter bodyExpr)
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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ComprehensionForClause;
            return o != null && Left.DoMatch(o.Left, match) && Target.DoMatch(o.Target, match)
                && Body.DoMatch(o.Body, match);
        }

        #endregion
	}

    /// <summary>
    /// Represents the if clause in a comprehension expression.
    /// </summary>
    public class ComprehensionIfClause : ComprehensionIter
	{
		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
        public Expression Condition{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
		}

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{
            get{return GetChildByRole(ComprehensionIter.CompBody);}
            set{SetChildByRole(ComprehensionIter.CompBody, value);}
		}

        public override NodeType NodeType{
            get{return NodeType.Expression;}
        }

		public ComprehensionIfClause(Expression test, ComprehensionIter bodyExpr)
		{
            Condition = test;
            Body = bodyExpr;
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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ComprehensionIfClause;
            return o != null && Condition.DoMatch(o.Condition, match) && Body.DoMatch(o.Body, match);
        }

        #endregion
	}
}

