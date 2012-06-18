using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;


namespace Expresso.Ast
{
	public class InExpression : Expression
	{
		public List<Expression> Expressions{get; internal set;}

		private int[] cur_poses;

		public InExpression(List<Expression> exprs)
		{
			Expressions = exprs;
			cur_poss = new int[exprs.Count];
		}

		public override NodeType Type
        {
            get { return NodeType.RValue; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as InExpression;

            if (x == null) return false;

            return this.Expressions.Equals(x.Expressions);
        }

        public override int GetHashCode()
        {
            return this.Expressions.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
			var objs = new List<ExpressoObj>(Expressions.Count);
			foreach (var item in Expressions) {
				objs.Add((ExpressoObj)item.Run(varStore, funcTable));
			}
			return ExpressoFunctions.MakeTuple(objs);
        }
	}
}

