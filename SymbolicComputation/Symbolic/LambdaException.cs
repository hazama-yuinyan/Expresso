using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expresso.Symbolic
{
	public class LambdaException : Exception
	{
		public LambdaException(string msg) : base(msg, null) { }
		public LambdaException(string msg, Exception innerException) :
			base(msg, innerException) { }
	}
}
