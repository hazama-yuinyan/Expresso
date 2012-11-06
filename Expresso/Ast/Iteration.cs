using System;
using System.Collections.Generic;
using System.Collections;
using Expresso.BuiltIns;
using Expresso.Interpreter;


namespace Expresso.Ast
{
	public class Iteration : Expression
	{
		public List<Expression> Targets{get; internal set;}

		public List<Expression> Expressions{get; internal set;}

		private IEnumerator[] enumerators = null;

		public override NodeType Type
        {
            get { return NodeType.Iteration; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Iteration;

            if (x == null) return false;

            return this.Targets.Equals(x.Targets) && this.Expressions.Equals(x.Expressions);
        }

        public override int GetHashCode()
        {
            return this.Targets.GetHashCode() ^ this.Expressions.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			if(enumerators == null){
				enumerators = new IEnumerator[Expressions.Count];
				for (int i = 0; i < Expressions.Count; ++i) {
					var iterable = Expressions[i].Run(varStore) as IEnumerable;
					if(iterable == null)
						throw new EvalException("{0} isn't evaluated to an iterable.", Expressions[i]);

					enumerators[i] = iterable.GetEnumerator();
				}
			}

			var objs = new List<object>(Expressions.Count);
			foreach (var item in enumerators) {
				if(!item.MoveNext()) continue;
				objs.Add(item.Current);
			}

			for (int i = 0; i < Targets.Count; ++i) {
				varStore.Assign(((Identifier)Targets[i].Run(varStore)).Name, objs[i]);
			}

			return null;
        }
	}
}

