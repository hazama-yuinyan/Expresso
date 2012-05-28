using System;

namespace Expresso.Interpreter
{
	public class EvalException : Exception
	{
		public EvalException (string msg) : base(msg)
		{
		}
	}
}

