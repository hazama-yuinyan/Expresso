using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// アサート文。
	/// The assert statement.
    /// </summary>
    public class AssertStatement : Statement
    {
        /// <summary>
        /// チェックする式。
        /// The test.
        /// </summary>
        public Expression Test{get; internal set;}

        /// <summary>
        /// Testがfalseになった時に表示するメッセージ。
		/// The message that will be displayed if the test fails.
        /// </summary>
        public Expression Message{get; internal set;}

        public override NodeType Type{
            get{return NodeType.AssertStatement;}
        }

        public override bool Equals(object obj)
        {
            var x = obj as AssertStatement;

            if(x == null)
                return false;

            return this.Test.Equals(x.Test)
                && this.Message.Equals(x.Message);
        }

        public override int GetHashCode()
        {
            return this.Test.GetHashCode() ^ this.Message.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			return null;
        }*/
		
		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				if(Test != null)
					Test.Walk(walker);

				if(Message != null)
					Message.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return string.Format("(Assert) {0} {1}", Test, Message);
		}
    }
}
