using System;

namespace Expresso.Runtime.Types
{
	/// <summary>
	/// Runtime Helperという名前のほうが良かったかも。
	/// Dynamic helpers.
	/// </summary>
	public static class DynamicHelpers
	{
		public static ExpressoType GetExpressoTypeFromType(Type type)
		{
			if(type == null)
				throw new ArgumentNullException("type");

			return ExpressoType.GetExpressoType(type);
		}

		public static ExpressoType GetExpressoType(object obj)
		{
			return GetExpressoTypeFromType(obj.GetType());
		}
	}
}

