using System;

namespace Expresso.Compiler
{
	/// <summary>
	/// C#Emitter内で発生した例外をあらわす。
	/// Represents an exception occurred while emitting native code.
	/// </summary>
	public class EmitterException : Exception
	{
		public EmitterException(string msg) : base("CSharpEmitter Error: " + msg)
		{
		}

		public EmitterException(string format, params object[] vals) : base(string.Format(format, vals))
		{}
	}
}

