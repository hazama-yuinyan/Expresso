using System;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	public class BreakStatement : Statement
	{
		/// <summary>
		/// breakの際に何階層分ループ構造を遡るか。
		/// </summary>
		public int Count{get; internal set;}

		/// <summary>
		/// このbreak文が含まれるループ構文。
		/// </summary>
		public Statement Enclosing{get; internal set;}

		public override NodeType Type
        {
            get { return NodeType.BreakStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as BreakStatement;

            if (x == null) return false;
			
			return Count.Equals(x.Count);
        }

        public override int GetHashCode()
        {
            return this.Count.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            /*var for_stmt = Enclosing as ForStatement;
			if(for_stmt == null){
				var while_stmt = Enclosing as WhileStatement;
				if(while_stmt == null)
					throw new EvalException("Something wrong has occurred! This \"break\" statement might be in a non-brekable block.");

				while_stmt
			}*/
			return null;
        }
	}

	public class ContinueStatement : Statement
	{
		public int Count{get; internal set;}

		public override NodeType Type
        {
            get { return NodeType.ContinueStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ContinueStatement;

            if (x == null) return false;
			
			return Count.Equals(x.Count);
        }

        public override int GetHashCode()
        {
            return this.Count.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
			return null;
        }
	}
}

