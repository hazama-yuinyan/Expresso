using System;

namespace Expresso.Interpreter
{
	public class EvalException : Exception
	{
		public EvalException (string msg) : base(msg)
		{
		}

		public EvalException(string format, params object[] objs) : base(string.Format(format, objs))
		{
		}
	}
}

