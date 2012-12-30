using System;

namespace Expresso.Runtime.Exceptions
{
	/// <summary>
	/// Represents an error indicating that an attempt to import a module results in failure.
	/// </summary>
	public class ImportException : Exception
	{
		public ImportException(string msg) : base(msg)
		{
		}

		public ImportException(string format, params object[] objs) : base(string.Format(format, objs))
		{
		}

		public ImportException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}

