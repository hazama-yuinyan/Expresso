using System;
using Expresso.Builtins;

namespace Expresso.Interpreter
{
	public class ExpressoThrowException : Exception
	{
		public ExpressoClass.ExpressoObj Thrown{get; internal set;}

		public ExpressoThrowException(ExpressoClass.ExpressoObj thrown) : base("")
		{
		}
	}
}

