using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
    /// <summary>
    /// 関数呼び出し。
	/// Reperesents a function call.
    /// </summary>
    public class Call : Expression
    {
        int num_args;

        /// <summary>
        /// 呼び出す対象。
		/// The target function to be called.
        /// It can be null if it is a call to a method, since methods are implemented in the way of
		/// objects having function objects and they are resolved at runtime.
        /// </summary>
        public Expression Target{
            get{return LastChild;}
		}

        /// <summary>
        /// 与える実引数リスト。
		/// The argument list to be supplied to the call.
        /// </summary>
        public IEnumerable<Expression> Arguments{
            get{return Children.Take(num_args);}
		}

		//public MethodContainer MethodInfo{get{return method_info;}}

		//MethodContainer method_info = null;

        public override NodeType Type{
            get{return NodeType.Call;}
        }

        public Call(Expression targetExpr, Expression[] arguments)
        {
            foreach(var arg in arguments)
                AddChild(arg);

            AddChild(targetExpr);
        }

        public override bool Equals(object obj)
        {
            var x = obj as Call;

            if(x == null)
                return false;

            if(this.Arguments.Count() != x.Arguments.Count())
                return false;

            return Arguments.SequenceEqual(x.Arguments);
        }

        public override int GetHashCode()
        {
            return this.Target.GetHashCode() ^ this.Arguments.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){
                Target.AcceptWalker(walker);
                foreach(var arg in Arguments)
                    arg.AcceptWalker(walker);
			}
			walker.PostWalk(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
        }

        public override string GetText()
		{
            return string.Format("<Call: {0} ({1})>", Target, Arguments);
		}

        public override string ToString()
        {
            return GetText();
        }
    }
}
