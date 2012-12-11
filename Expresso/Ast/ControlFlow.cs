using System;
using System.Collections.Generic;

using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	public abstract class BreakableStatement : Statement
	{
		protected bool can_continue = true;

		public bool CanContinue
		{
			get {return this.can_continue;}
			internal set {this.can_continue = value;}
		}
	}

	public class BreakStatement : Statement
	{
		/// <summary>
		/// breakの際に何階層分ループ構造を遡るか。
		/// Indicates how many loops we will break out.
		/// </summary>
		public int Count{get; internal set;}

		/// <summary>
		/// このbreak文が含まれるループ構文。
		/// Loops that have this statement as a child.
		/// </summary>
		public List<BreakableStatement> Enclosings{get; internal set;}

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

        internal override object Run(VariableStore varStore)
        {
			foreach(var enclosing in Enclosings){
				enclosing.CanContinue = false;
			}

			return null;
        }

		internal override System.Linq.Expressions.Expression Compile(Emitter emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString ()
		{
			return string.Format ("break upto {0}", Count);
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

        internal override object Run(VariableStore varStore)
        {
			return null;
        }

		internal override System.Linq.Expressions.Expression Compile(Emitter emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString ()
		{
			return string.Format ("continue on {0} up", Count);
		}
	}
}

