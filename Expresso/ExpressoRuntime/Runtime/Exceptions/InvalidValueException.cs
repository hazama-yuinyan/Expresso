using System;

namespace Expresso.Runtime.Exceptions
{
	/// <summary>
	/// 不正な値が使用されていることをあらわす。
	/// Represents an error indicating that a paticular value is invalid.
	/// </summary>
	public class InvalidValueException : Exception
	{
		public InvalidValueException(string typeName) : base(typeName)
		{
		}

		public InvalidValueException(string format, params object[] objs) : base(string.Format(format, objs))
		{
		}

		public InvalidValueException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}

