using System;

namespace Expresso.Runtime.Exceptions
{
	/// <summary>
	/// 対象の動作に対して型が合致しないことをあらわす。
	/// Represents an error indicating that the type is not valid for the operation, action or something.
	/// </summary>
	public class InvalidTypeException : Exception
	{
		public InvalidTypeException(string msg) : base(msg)
		{
		}
		
		public InvalidTypeException(string format, params object[] objs) : base(string.Format(format, objs))
		{
		}

		public InvalidTypeException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}

