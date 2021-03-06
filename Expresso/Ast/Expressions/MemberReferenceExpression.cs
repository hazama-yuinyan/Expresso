using System;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
	/// <summary>
	/// Represents a member reference.
    /// A member reference expression references a member or method of an object.
    /// Expression '.' Identifier ;
	/// </summary>
    public class MemberReferenceExpression : Expression
	{
		/// <summary>
		/// The target expression of which a member will be referenced.
		/// </summary>
		public Expression Target{
            get => GetChildByRole(Roles.TargetExpression);
            set => SetChildByRole(Roles.TargetExpression, value);
		}

        public ExpressoTokenNode DotToken => GetChildByRole(Roles.DotToken);

		/// <summary>
		/// The member expression.
		/// </summary>
        public Identifier Member{
            get => GetChildByRole(Roles.Identifier);
            set => SetChildByRole(Roles.Identifier, value);
		}

        public MemberReferenceExpression(Expression targetExpr, Identifier member)
            : base(targetExpr.StartLocation, member.EndLocation)
		{
            Target = targetExpr;
            Member = member;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitMemberReference(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitMemberReference(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitMemberReference(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as MemberReferenceExpression;
            return o != null && Target.DoMatch(o.Target, match) && Member.DoMatch(o.Member, match);
        }

        #endregion
	}
}

