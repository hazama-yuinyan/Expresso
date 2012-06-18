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

