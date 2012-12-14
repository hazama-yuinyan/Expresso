using System;
using System.Collections.Generic;

using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// break,continue文でコントロールできる文。
	/// Represents a breakable statement, which is the statement on which a break or a continue statement has its effect.
	/// </summary>
	public abstract class BreakableStatement : Statement
	{
		protected bool can_continue = true;

		public bool CanContinue
		{
			get {return this.can_continue;}
			internal set {this.can_continue = value;}
		}
	}

	/// <summary>
	/// break文。
	/// The break statement.
	/// </summary>
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
			foreach(var enclosing in Enclosings)	//Count階層分ループを遡るまで出会ったbreakableに中断命令を出す
				enclosing.CanContinue = false;

			return null;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString ()
		{
			return string.Format ("break upto {0}", Count);
		}
	}

	/// <summary>
	/// continue文。
	/// The continue statement.
	/// </summary>
	public class ContinueStatement : Statement
	{
		/// <summary>
		/// continueの際に何階層分ループ構造を遡るか。
		/// Indicates how many loops we will break out.
		/// </summary>
		public int Count{get; internal set;}

		/// <summary>
		/// continue文が含まれるループ構文。
		/// Loops that have this statement as a child.
		/// </summary>
		public List<BreakableStatement> Enclosings{get; internal set;}

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
			foreach(var enclosing in Enclosings)	//Note:continueすべきループは構文木生成段階で除かれているので
				enclosing.CanContinue = false;		//単純に内包されているbreakableに中断命令を出すだけでいい

			return null;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString ()
		{
			return string.Format("continue on {0} up", Count);
		}
	}
}

