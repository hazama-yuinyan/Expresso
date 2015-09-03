using System;


namespace Expresso.Ast
{
	/// <summary>
	/// Represents a member reference.
    /// A member reference expression references a member or method of an object.
    /// Expression '.' Identifier ;
	/// </summary>
    public class MemberReference : Expression
	{
		/// <summary>
		/// The target expression of which a member will be referenced.
		/// </summary>
		public Expression Target{
            get{return GetChildByRole(Roles.TargetExpression);}
            set{SetChildByRole(Roles.TargetExpression, value);}
		}

        public ExpressoTokenNode DotToken{
            get{return GetChildByRole(Roles.DotToken);}
        }

		/// <summary>
		/// The member expression.
		/// </summary>
        public Identifier Member{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
		}

        public MemberReference(Expression targetExpr, Identifier member)
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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as MemberReference;
            return o != null && Target.DoMatch(o.Target, match) && Member.DoMatch(o.Member, match);
        }

        #endregion
	}
}

