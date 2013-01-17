using System;

namespace Expresso.Utils
{
	public static class Utils
	{
		public static ArgumentNullException MakeArgumentItemNullException(int index, string arrayName)
		{
			return new ArgumentNullException(String.Format("{0}[{1}]", arrayName, index));
		}
	}
}

