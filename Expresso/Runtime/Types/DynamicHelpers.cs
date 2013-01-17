using System;

namespace Expresso.Runtime.Types
{
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

