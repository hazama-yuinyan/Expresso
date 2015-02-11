using System;


namespace Expresso.Ast
{
	/// <summary>
	/// Represents a member reference.
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
		/// The subscript expression.
		/// </summary>
        public Identifier Subscription{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
		}

        public MemberReference(Expression targetExpr, Identifier subscript)
		{
            Target = targetExpr;
            Subscription = subscript;
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
            return o != null && Target.DoMatch(o.Target, match) && Subscription.DoMatch(o.Subscription, match);
        }

        #endregion

        /*internal override void Assign( frame, object val)
        //{
			/*var obj = Parent.Run(varStore);
			if(obj == null)
				throw ExpressoOps.InvalidTypeError("Can not evaluate the name to a valid Expresso object");

			var subscript = Subscription.Run(varStore);
			if(obj is ExpressoObj){
				if(subscript is Identifier){
					var ident = (Identifier)subscript;
					((ExpressoObj)obj).Assign(ident, val, obj == varStore.Get(0, 0));
				}else{
					throw ExpressoOps.RuntimeError("Invalid assignment!");
				}
			}else{
				ExpressoOps.AssignToCollection(obj, subscript, val);
			}
        }*/
	}
}

