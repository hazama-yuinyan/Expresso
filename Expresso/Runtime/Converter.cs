using System;

using Expresso.Runtime.Operations;
using Expresso.Runtime.Types;

namespace Expresso.Runtime
{
	public class Converter
	{
		public Converter()
		{
		}

		public static Type ConvertToType(object value)
		{
			if(value == null) return null;
			
			Type type_val = value as Type;
			if(type_val != null) return type_val;
			
			ExpressoType exs_type_val = value as ExpressoType;
			if(exs_type_val != null) return exs_type_val.UnderlyingSystemType;
			
			/*TypeGroup typeCollision = value as TypeGroup;
			if (typeCollision != null) {
				Type nonGenericType;
				if (typeCollision.TryGetNonGenericType(out nonGenericType)) {
					return nonGenericType;
				}
			}*/
			
			throw ExpressoOps.TypeErrorForTypeMismatch("Type", value);
		}
	}
}

