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

	public class MemberReference : Assignable
	{
		public Expression Parent{get; internal set;}

		public Expression Subscription{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.MemRef; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as MemberReference;

            if (x == null) return false;

            return this.Parent.Equals(x.Parent) && this.Subscription.Equals(x.Subscription);
        }

        public override int GetHashCode()
        {
            return this.Parent.GetHashCode() ^ this.Subscription.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
            var obj = Parent.Run(varStore);

			var subscription = Subscription.Run(varStore);
			if(subscription is ExpressoIntegerSequence){
				var seq = (ExpressoIntegerSequence)subscription;
				return ImplementationHelpers.Slice(obj, seq);
			}

			if(obj is ExpressoObj){
				var exs_obj = (ExpressoObj)obj;
				var member = exs_obj.AccessMember(subscription, obj == varStore.Get(0, 0));
				return (member is Function) ? new MethodContainer(member as Function, obj) : member;
			}else{
				var member = ImplementationHelpers.AccessMember((Identifier)Parent, obj, subscription);
				return (member is Function) ? new MethodContainer(member as Function, obj) : member;
			}
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Assign(VariableStore varStore, object val)
		{
			var obj = Parent.Run(varStore);
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
				ImplementationHelpers.AssignToCollection(obj, subscript, val);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}", Parent, Subscription);
		}
	}
}

