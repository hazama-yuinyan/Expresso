using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
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
        public Expression Test{
            get{
                return (Expression)FirstChild;
            }
        }

        /// <summary>
        /// Testがfalseになった時に表示するメッセージ。
		/// The message that will be displayed if the test fails.
        /// </summary>
        public Expression Message{
            get{
                return (Expression)FirstChild.NextSibling;
            }
        }

        public override NodeType Type{
            get{return NodeType.AssertStatement;}
        }

        public AssertStatement(Expression test, Expression message)
        {
            AddChild(test);
            AddChild(message);
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

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){
				if(Test != null)
                    Test.AcceptWalker(walker);

				if(Message != null)
                    Message.AcceptWalker(walker);
			}
			walker.PostWalk(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
        }

        public override string GetText()
		{
            return string.Format("<Assert: {0} {1}>", Test, Message);
		}

        public override string ToString()
        {
            return GetText();
        }
    }
}
