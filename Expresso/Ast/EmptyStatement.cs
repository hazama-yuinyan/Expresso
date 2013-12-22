using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// 空の文。
	/// Reperesents a empty statement.
    /// </summary>
    public class EmptyStatement : Statement
    {
		public EmptyStatement()
		{
		}

        public override NodeType Type{
            get{return NodeType.EmptyStatement;}
        }

        public override bool Equals(object obj)
        {
            var x = obj as EmptyStatement;

            if(x == null)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return 0xffefce;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return string.Format("[Empty statement]");
		}
    }
}
