using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;


namespace Expresso.Ast
{
	public class MemberReference : Expression
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

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            var obj = Parent.Run(varStore, funcTable) as ExpressoObj;
			if(obj == null)
				throw new EvalException("Can not evaluate the object to a valid one.");

			var subscription = (ExpressoObj)Subscription.Run(varStore, funcTable);
			if(subscription is ExpressoIntegerSequence)
				return ((ExpressoContainer)obj).Slice((ExpressoIntegerSequence)subscription);

			return obj.AccessMember(subscription);
        }
	}
}

