using System;

namespace Expresso.Runtime
{
	/// <summary>
	/// C#の型をExpressoの型として指定する属性。
    /// この属性が指定された型は、Expresso内の文法に合わせてプロパティがメソッドに置き換えられたり、メソッド名のキャメルケース化
    /// スライスオペレーションの定義などが行われる。
	/// Marks a type as being an ExpressoType for purposes of member lookup, creating instances, etc...  
    /// It also indicates that types marked with this attribute could be modified so that they suit
    /// the Expresso world. For example, we don't have properties in Expresso so this attribute compells
    /// marked types to get rid of properties and to define setters and getters as methods.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, Inherited = false)]
	public sealed class ExpressoTypeAttribute : Attribute
	{
		readonly string type_name;
		
		public ExpressoTypeAttribute()
		{
		}
		
        /// <summary>
        /// Creates a new <see cref="Expresso.Runtime.ExpressoTypeAttribute"/> class instance.
        /// </summary>
        /// <param name="name">The name by which the corresponding type is referred to in Expresso.</param>
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

