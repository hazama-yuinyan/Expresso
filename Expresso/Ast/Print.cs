using System;
using System.Collections.Generic;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	public class PrintStatement : Statement
	{
		public List<Expression> Expressions{get; internal set;}
		
		public override NodeType Type
        {
            get { return NodeType.Print; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as PrintStatement;

            if (x == null) return false;

            return this.Expressions.Equals(x.Expressions);
        }

        public override int GetHashCode()
        {
            return this.Expressions.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
			foreach (var expr in Expressions) {
				object obj = expr.Run(varStore, funcTable);
				Console.Write("{0} ", obj.ToString());
			}
			Console.WriteLine();
			return null;
        }
	}
}

