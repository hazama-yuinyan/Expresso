using System;

using Expresso.Runtime;

namespace Expresso.Interpreter
{
	/// <summary>
	/// Expressoのソース内で投げられた例外をあらわす。
	/// Represents exceptions thrown in Expresso.
	/// </summary>
	public class ExpressoThrowException : Exception
	{
		public ExpressoObj Thrown{get; internal set;}

		public ExpressoThrowException(ExpressoObj thrown) : base("")
		{
		}
	}
}

