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
            var obj = Parent.Run(varStore);
			if(obj == null)
				throw new EvalException("Can not evaluate the object to a valid one.");

			var subscription = Subscription.Run(varStore);
			if(subscription is ExpressoIntegerSequence){
				var seq = (ExpressoIntegerSequence)subscription;
				if(obj is List<object>)
					return ((List<object>)obj).Slice(seq);
				else if(obj is ExpressoContainer)
					return ((ExpressoContainer)obj).Slice(seq);
				else if(obj is Array)
					return ((object[])obj).Slice(seq);
			}

			if(obj is List<object>){
				if(!(subscription is int))
					throw new EvalException("Can not evaluate the subscription to an int value");

				int index = (int)subscription;
				return ((List<object>)obj)[index];
			}else if(obj is Dictionary<object, object>){
				object value = null;
				((Dictionary<object, object>)obj).TryGetValue(subscription, out value);
				return value;
			}else if(obj is Array){
				if(!(subscription is int))
					throw new EvalException("Can not evaluate the subscription to an int value.");

				int index = (int)subscription;
				return ((object[])obj)[index];
			}else if(obj is ExpressoTuple){
				if(!(subscription is int))
					throw new EvalException("Can not evaluate the subscription to an int value.");

				int index = (int)subscription;
				return ((ExpressoTuple)obj).Contents[index];
			}

			return null;
        }
	}
}

