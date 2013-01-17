using System;

namespace Expresso.Runtime.Exceptions
{
	/// <summary>
	/// Represents an error indicating that an assertion failed.
	/// </summary>
	public class AssertionFailedException : Exception
	{
		public AssertionFailedException(string msg) : base(msg)
		{
		}
		
		public AssertionFailedException(string format, params object[] objs) : base(string.Format(format, objs))
		{
		}
		
		public AssertionFailedException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}

