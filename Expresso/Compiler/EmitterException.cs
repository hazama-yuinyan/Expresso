using System;

namespace Expresso.Compiler
{
	public class EmitterException : Exception
	{
		public EmitterException(string msg) : base("CSharpEmitter Error: " + msg)
		{
		}

		public EmitterException(string format, params object[] vals) : base(string.Format(format, vals))
		{}
	}
}

