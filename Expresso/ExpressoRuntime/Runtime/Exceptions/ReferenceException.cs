using System;

namespace Expresso.Runtime.Exceptions
{
	/// <summary>
	/// 不正な参照が使用されていることをあらわす。(束縛前の参照を使用しているなど)
	/// Represents an error indicating that a reference is invalid or that a reference actually points to nothing etc.
	/// </summary>
	public class ReferenceException : Exception
	{
		public ReferenceException(string msg) : base(msg)
		{
		}

		public ReferenceException(string format, params object[] objs) : base(string.Format(format, objs))
		{
		}

		public ReferenceException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}

