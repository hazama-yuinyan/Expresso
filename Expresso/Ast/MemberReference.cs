using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;
using Expresso.Helpers;


namespace Expresso.Ast
{
	public class MemberReference : Assignable
	{
		public Expression Parent{get; internal set;}

		public Expression Subscription { get; internal set; }

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
            var obj = Parent.Run(varStore) as ExpressoClass.ExpressoObj;
			if(obj == null)
				throw new EvalException("Can not evaluate the expression to a valid object.");

			var subscription = Subscription.Run(varStore);
			var subscript_obj = subscription as ExpressoClass.ExpressoObj;
			if(subscript_obj != null && subscript_obj.Type == TYPES.SEQ){
				var seq = (ExpressoIntegerSequence)subscript_obj.GetMember(0);
				return obj.Slice(seq);
			}

			var member = obj.AccessMember(subscription, obj == varStore.Get(0, 0));
			return (member is Function) ? new MethodContainer(member as Function, obj) : member;
        }

		internal override void Assign(VariableStore varStore, object val)
		{
			var obj = Parent.Run(varStore) as ExpressoClass.ExpressoObj;
			if(obj == null)
				throw new EvalException("Can not evaluate the name to a valid Expresso object");

			var subscript = Subscription.Run(varStore);
			if(subscript is Identifier){
				var ident = (Identifier)subscript;
				obj.Assign(ident, val, obj == varStore.Get(0, 0));
			}else if(subscript is int){
				obj.Assign((int)subscript, val);
			}else{
				obj.Assign(subscript, val);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}", Parent, Subscription);
		}
	}
}

