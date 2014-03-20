using System;

namespace Expresso.Runtime.Exceptions
{
	/// <summary>
	/// ユーザーコードではなく、Expressoの実装により発生したエラーをあらわす。
	/// Represents an error has occurred during the language operations.
	/// </summary>
	public class ExpressoSystemException : Exception
	{
		public ExpressoSystemException(string msg) : base(msg)
		{
		}

		public ExpressoSystemException(string format, params object[] objs) : base(string.Format(format, objs))
		{
		}

		public ExpressoSystemException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}

