using System;

namespace Expresso.Runtime.Exceptions
{
	/// <summary>
	/// 実行時エラーをあらわす。
    /// Represents the panicked state as a native exception.
	/// </summary>
	public class PanickedException : Exception
	{
		public PanickedException(string msg) : base(msg)
		{
		}

		public PanickedException(string format, params object[] objs)
            : base(string.Format(format, objs))
		{
		}

		public PanickedException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}

