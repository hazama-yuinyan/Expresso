using System;
using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Runtime.Operations;


namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// Represents a member reference.
	/// </summary>
	public class MemberReference : Assignable
	{
		Expression target;
		Expression subscription;

		/// <summary>
		/// The target expression of which a member will be referenced.
		/// </summary>
		public Expression Target{
			get{return target;}
		}

		/// <summary>
		/// The subscript expression. It can be an identifier or index.
		/// </summary>
		public Expression Subscription{
			get{return subscription;}
		}

        public override NodeType Type
        {
            get { return NodeType.MemRef; }
        }

		public MemberReference(Expression targetExpr, Expression subscriptExpr)
		{
			target = targetExpr;
			subscription = subscriptExpr;
		}

        public override bool Equals(object obj)
        {
            var x = obj as MemberReference;

            if (x == null) return false;

            return this.target.Equals(x.target) && this.subscription.Equals(x.subscription);
        }

        public override int GetHashCode()
        {
            return this.target.GetHashCode() ^ this.subscription.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
            var obj = Parent.Run(varStore);

			var subscription = Subscription.Run(varStore);
			if(subscription is ExpressoIntegerSequence){
				var seq = (ExpressoIntegerSequence)subscription;
				return ExpressoOps.Slice(obj, seq);
			}

			if(obj is ExpressoObj){
				var exs_obj = (ExpressoObj)obj;
				var member = exs_obj.AccessMember(subscription, obj == varStore.Get(0, 0));
				return (member is FunctionDeclaration) ? new MethodContainer(member as FunctionDeclaration, obj) : member;
			}else{
				var member = ExpressoOps.AccessMember((Identifier)Parent, obj, subscription);
				return (member is FunctionDeclaration) ? new MethodContainer(member as FunctionDeclaration, obj) : member;
			}
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				target.Walk(walker);
				subscription.Walk(walker);
			}
			walker.PostWalk(this);
		}

		internal override void Assign(EvaluationFrame frame, object val)
		{
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
			}*/
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}", target, subscription);
		}
	}
}

