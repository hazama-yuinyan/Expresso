using System;
using System.Collections.Generic;
using System.Text;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// The Print statement.
	/// </summary>
	public class PrintStatement : Statement
	{
		public List<Expression> Expressions{get; internal set;}

		public bool HasTrailing{get; internal set;}
		
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

        internal override object Run(VariableStore varStore)
        {
			var first = Expressions[0].Run(varStore);
			if(first is string){
				var values = new List<object>();
				var sb = new StringBuilder((string)first);
				for(int i = 1; i < Expressions.Count; ++i){
					values.Add(Expressions[i].Run(varStore));
					sb.Append("%s");
					if(i + 1 != Expressions.Count)
						sb.Append(",");
				}

				var text = ExpressoFunctions.Format(sb.ToString(), values.ToArray());
				if(!HasTrailing)
					Console.WriteLine(text);
				else
					Console.Write(text);
			}else{
				Console.Write(first);
				bool print_comma = true;
				for(int i = 1; i < Expressions.Count; ++i){
					if(print_comma)
						Console.Write(",");

					object obj = Expressions[i].Run(varStore);
					Console.Write(obj);
					print_comma = (obj is string) ? false : true;
				}
				if(!HasTrailing)
					Console.WriteLine();
				else
					Console.Write(",");
			}

			return null;
        }
	}
}

