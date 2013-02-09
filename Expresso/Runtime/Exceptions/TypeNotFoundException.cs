using System;

namespace Expresso.Runtime.Exceptions
{
	/// <summary>
	/// 対象の型が見つからないことをあらわす。
	/// Represents an error indicating that a paticular type is missing.
	/// </summary>
	public class TypeNotFoundException : Exception
	{
		public TypeNotFoundException(string typeName) : base(typeName)
		{
		}

		public TypeNotFoundException(string format, params object[] objs) : base(string.Format(format, objs))
		{
		}

		public TypeNotFoundException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}

