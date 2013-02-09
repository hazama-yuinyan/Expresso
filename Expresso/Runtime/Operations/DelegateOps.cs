using System;

namespace Expresso.Runtime.Operations
{
	/// <summary>
	/// デリゲートに対する処理。
	/// Delegate ops.
	/// </summary>
	public static class DelegateOps
	{
	}

	public interface IDelegateConvertible
	{
		Delegate ConvertToDelegate(Type type);
	}
}

