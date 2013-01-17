using System;

namespace Expresso.Runtime.Operations
{
	public static class DelegateOps
	{
	}

	public interface IDelegateConvertible
	{
		Delegate ConvertToDelegate(Type type);
	}
}

