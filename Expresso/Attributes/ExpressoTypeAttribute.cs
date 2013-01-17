using System;

namespace Expresso.Runtime
{
	/// <summary>
	/// Marks a type as being an ExpressoType for purposes of member lookup, creating instances, etc...  
	/// 
	/// If defined a ExpressoType will use op_new / constructor when creating instances. This allows the
	/// object to match the native Python behavior such as returning cached values from op_new or
	/// supporting initialization to run multiple times via constructor.
	///
	/// The attribute also allows you to specify an alternate type name. This allows the .NET name to
	/// be different from the Expresso name so they can follow .NET naming conventions.
	/// 
	/// Types defining this attribute also don't show CLR methods such as Equals, GetHashCode, etc... until
	/// the user has done an import clr.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, Inherited = false)]
	public sealed class ExpressoTypeAttribute : Attribute
	{
		private readonly string type_name;
		
		public ExpressoTypeAttribute()
		{
		}
		
		public ExpressoTypeAttribute(string name)
		{
			type_name = name;
		}
		
		public string Name{
			get {
				return type_name;
			}
		}
	}
}

