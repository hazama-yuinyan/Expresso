using System;
using Expresso.Ast;

namespace Expresso.CodeGen
{
	/// <summary>
    /// CSharpEmitter内で発生した例外をあらわす。
	/// Represents an exception occurred while emitting native code.
	/// </summary>
	public class EmitterException : Exception
	{
        public AstNode Node{
            get; set;
        }

		public EmitterException(string msg) : base("CSharpEmitter Error: " + msg)
		{
		}

		public EmitterException(string format, params object[] vals)
            : base(string.Format(format, vals))
		{
        }

        public EmitterException(string format, AstNode node, params object[] values)
            : this(format, values)
        {
            Node = node;
        }
	}
}

