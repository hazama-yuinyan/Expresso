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

        internal override object Run(VariableStore varStore)
        {
            var obj = Parent.Run(varStore) as ExpressoClass.ExpressoObj;
			if(obj == null)
				throw new EvalException("Can not evaluate the expression to a valid object.");

			var subscription = Subscription.Run(varStore);
			if(subscription is ExpressoIntegerSequence){
				var seq = (ExpressoIntegerSequence)subscription;
				switch(obj.Type){
				case TYPES.LIST:
					return ((List<object>)obj.GetMember(0)).Slice(seq);

				case TYPES.TUPLE:
					return ((ExpressoTuple)obj.GetMember(0)).Slice(seq);

				case TYPES.ARRAY:
					return ((object[])obj.GetMember(0)).Slice(seq);

				default:
					throw new EvalException("Can not apply the slice operation on that type of object!");
				}
			}

			return obj.AccessMember(subscription);
        }

		public override string ToString()
		{
			return string.Format("{0}.{1}", Parent, Subscription);
		}
	}
}

