using System;
using System.Diagnostics;

using Expresso.Utils;

namespace Expresso.Runtime.Meta
{
	/// <summary>
	/// Convention for an individual argument at a callsite.
	/// 
	/// Multiple different callsites can match against a single declaration. 
	/// Some argument kinds can be "unrolled" into multiple arguments, such as list and dictionary. 
	/// </summary>
	public enum ArgumentType
	{
		/// <summary>
		/// Simple unnamed positional argument.
		/// In Python: foo(1,2,3) are all simple arguments.
		/// </summary>
		Simple,
		
		/// <summary>
		/// Argument with associated name at the callsite
		/// In Python: foo(a=1)
		/// </summary>
		Named,
		
		/// <summary>
		/// Argument containing a list of arguments. 
		/// In Python: foo(*(1,2*2,3))  would match 'def foo(a,b,c)' with 3 declared arguments such that (a,b,c)=(1,4,3).
		///      it could also match 'def foo(*l)' with 1 declared argument such that l=(1,4,3)
		/// </summary>
		List,
		
		/// <summary>
		/// Argument containing a dictionary of named arguments.
		/// In Python: foo(**{'a':1, 'b':2})
		/// </summary>
		Dictionary,
		
		
		Instance
	};

	/// <summary>
	/// 引数に関するメタデータを保持する。
	/// Argument info.
	/// </summary>
	public struct ArgumentInfo
	{
		readonly ArgumentType kind;
		readonly string name;
		
		public static readonly ArgumentInfo Simple = new ArgumentInfo(ArgumentType.Simple, null);
		
		public ArgumentType Kind { get { return kind; } }
		public string Name { get { return name; } }
		
		public ArgumentInfo(string argName)
		{
			kind = ArgumentType.Named;
			name = argName;
		}
		
		public ArgumentInfo(ArgumentType argKind)
		{
			kind = argKind;
			name = null;
		}
		
		public ArgumentInfo(ArgumentType argKind, string argName)
		{
			ContractUtils.Requires((argKind == ArgumentType.Named) ^ (argName == null), "kind");
			kind = argKind;
			name = argName;
		}
		
		public override bool Equals(object obj)
		{
			return obj is ArgumentInfo && Equals((ArgumentInfo)obj);
		}
		
		public bool Equals(ArgumentInfo other)
		{
			return kind == other.kind && name == other.name;
		}
		
		public static bool operator ==(ArgumentInfo left, ArgumentInfo right)
		{
			return left.Equals(right);
		}
		
		public static bool operator !=(ArgumentInfo left, ArgumentInfo right)
		{
			return !left.Equals(right);
		}
		
		public override int GetHashCode()
		{
			return (name != null) ? name.GetHashCode() ^ (int)kind : (int)kind;
		}
		
		public bool IsSimple{
			get {
				return Equals(Simple);
			}
		}
		
		public override string ToString()
		{
			return name == null ? kind.ToString() : kind.ToString() + ":" + name;
		}
		
		/*internal Expression CreateExpression()
		{
			return Expression.New(
				typeof(ArgumentInfo).GetConstructor(new Type[] { typeof(ArgumentType), typeof(string) }),
				AstUtils.Constant(kind),
				AstUtils.Constant(name, typeof(string))
			);
		}*/
	}
}

