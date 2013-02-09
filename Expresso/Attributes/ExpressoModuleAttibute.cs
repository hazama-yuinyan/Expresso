using System;

using Expresso.Utils;

namespace Expresso.Runtime
{
	/// <summary>
	/// ある型をExpressoの組み込みモジュールとして扱うための属性。この属性が指定された型は、アセンブリーのロード時に
	/// 自動的にExpressoのモジュールとしてRuntimeに登録される。
	/// This assembly-level attribute specifies which types in the engine represent built-in Expresso modules.
	/// 
	/// Members of a built-in module type should all be static as an instance is never created.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class ExpressoModuleAttribute : Attribute
	{
		private readonly string module_name;
		private readonly Type type;
		
		/// <summary>
		/// Creates a new ExpressoModuleAttribute that can be used to specify a built-in module that exists
		/// within an assembly.
		/// </summary>
		/// <param name="name">The built-in module name</param>
		/// <param name="type">The type that implements the built-in module.</param>
		public ExpressoModuleAttribute(string name, Type givenType)
		{
			ContractUtils.RequiresNotNull(name, "name");
			ContractUtils.RequiresNotNull(givenType, "givenType");
			
			module_name = name;
			type = givenType;
		}
		
		/// <summary>
		/// The built-in module name
		/// </summary>
		public string Name {
			get { return module_name; }
		}
		
		/// <summary>
		/// The type that implements the built-in module
		/// </summary>
		public Type Type {
			get { return type; }
		}
	}
}

