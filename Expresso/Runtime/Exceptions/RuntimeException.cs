using System;

namespace Expresso.Runtime.Exceptions
{
	/// <summary>
	/// 実行時エラーをあらわす。
	/// Represents an error has occurred during the program execution.
	/// </summary>
	public class RuntimeException : Exception
	{
		public RuntimeException(string msg) : base(msg)
		{
		}

		public RuntimeException(string format, params object[] objs) : base(string.Format(format, objs))
		{
		}

		public RuntimeException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}

