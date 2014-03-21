using System;
using System.Collections.Generic;

using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	/// <summary>
	/// break,continue文でコントロールできる文。
	/// Represents a breakable statement, which is the statement on which a break or a continue statement has its effect.
	/// </summary>
	public abstract class BreakableStatement : Statement
	{
		protected bool can_continue = true;

        public bool CanContinue{
			get{return this.can_continue;}
			internal set{this.can_continue = value;}
		}
	}

	/// <summary>
	/// break文。
	/// The break statement.
	/// </summary>
	public class BreakStatement : Statement
	{
		readonly int count;

		/// <summary>
		/// breakの際に何階層分ループ構造を遡るか。
		/// Indicates how many loops we will break out.
		/// </summary>
		public int Count{
			get{return count;}
		}

		/// <summary>
		/// このbreak文が含まれるループ構文。
		/// Loops that have this statement as their child.
		/// </summary>
        public IEnumerable<BreakableStatement> Enclosings{
            get{return Children;}
		}

        public override NodeType Type{
            get{return NodeType.BreakStatement;}
        }

		public BreakStatement(int loopCount, BreakableStatement[] loops)
		{
			count = loopCount;
			
            foreach(var enclosing in loops)
                AddChild(enclosing);
		}

        public override bool Equals(object obj)
        {
            var x = obj as BreakStatement;

            if(x == null)
                return false;
			
			return count.Equals(x.count);
        }

        public override int GetHashCode()
        {
            return this.count.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){}
			walker.PostWalk(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
        }

        public override string GetText()
		{
            return string.Format("<Break upto {0}>", Count);
		}

        public override string ToString()
        {
            return GetText();
        }
	}

	/// <summary>
	/// continue文。
	/// The continue statement.
	/// </summary>
	public class ContinueStatement : Statement
	{
		readonly int count;

		/// <summary>
		/// continueの際に何階層分ループ構造を遡るか。
		/// Indicates how many loops we will break out.
		/// </summary>
		public int Count{
			get{return count;}
		}

		/// <summary>
		/// continue文が含まれるループ構文。
		/// Loops that have this statement as their child.
		/// </summary>
        public IEnumerable<BreakableStatement> Enclosings{
            get{return Children;}
		}

        public override NodeType Type{
            get{return NodeType.ContinueStatement;}
        }

		public ContinueStatement(int loopCount, BreakableStatement[] loops)
		{
			count = loopCount;
			
            foreach(var enclosing in loops)
                AddChild(enclosing);
		}

        public override bool Equals(object obj)
        {
            var x = obj as ContinueStatement;

            if(x == null)
                return false;
			
			return count.Equals(x.count);
        }

        public override int GetHashCode()
        {
            return this.count.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){}
			walker.PostWalk(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
        }

        public override string GetText()
		{
            return string.Format("<Continue on {0} up>", Count);
		}

        public override string ToString()
        {
            return GetText();
        }
	}
}

