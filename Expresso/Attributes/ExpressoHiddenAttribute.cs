using System;

namespace Expresso.Runtime
{
	/// <summary>
	/// フィールドやクラスをExpressoのコードから見えないように指定する属性。
	/// Marks a member as being hidden from Expresso code.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal sealed class ExpressoHiddenAttribute : Attribute{
	}
}

