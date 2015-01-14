using System;
using System.Collections.Generic;

using Expresso.Runtime;
using Expresso.Compiler;


namespace Expresso.Ast
{
	/// <summary>
	/// Represents a member reference.
	/// </summary>
    public class MemberReference : Expression
	{
		/// <summary>
		/// The target expression of which a member will be referenced.
		/// </summary>
		public Expression Target{
            get{return GetChildByRole(Roles.TargetExpression);}
            set{SetChildByRole(Roles.TargetExpression);}
		}

        public ExpressoTokenNode DotToken{
            get{return GetChildByRole(Roles.DotToken);}
        }

		/// <summary>
		/// The subscript expression. It can be an identifier.
		/// </summary>
		public Expression Subscription{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
		}

        public override NodeType NodeType{
            get{return NodeType.Expression;}
        }

		public MemberReference(Expression targetExpr, Expression subscriptExpr)
		{
            Target = targetExpr;
            Subscription = subscriptExpr;
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

