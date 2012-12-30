using System;
using System.Collections.Generic;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

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
			var sb = new StringBuilder();
			if(first is string){
				var values = new List<object>(Expressions.Count);
				sb.Append(first);
				for(int i = 1; i < Expressions.Count; ++i){
					values.Add(Expressions[i].Run(varStore));
					sb.Append("{" + (i - 1) + "}");
					if(i + 1 != Expressions.Count)
						sb.Append(",");
				}

				var text = string.Format(sb.ToString(), values.ToArray());
				if(!HasTrailing)
					Console.WriteLine(text);
				else
					Console.Write(text);
			}else{
				sb.Append(first);
				bool print_comma = true;
				for(int i = 1; i < Expressions.Count; ++i){
					if(print_comma)
						sb.Append(",");

					object obj = Expressions[i].Run(varStore);
					sb.Append(obj);
					print_comma = (obj is string) ? false : true;
				}

				var text = sb.ToString();
				if(!HasTrailing)
					Console.WriteLine(text);
				else
					Console.Write(text + ",");
			}

			return null;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}
	}
}

