using System;
using System.Collections.Generic;
using System.Numerics;

using Expresso.Runtime;
using Expresso.Utils;

namespace Expresso.Runtime.Binding
{
	public class ExpressoBinder
	{
		private static readonly Dictionary<Type, ExtensionTypeInfo> sys_types = MakeSystemTypes();

		public ExpressoBinder()
		{
		}

		private class ExtensionTypeInfo
		{
			public Type ExtensionType;
			public string PythonName;
			
			public ExtensionTypeInfo(Type extensionType, string pythonName)
			{
				ExtensionType = extensionType;
				PythonName = pythonName;
			}
		}

		/// <summary>
		/// Creates a table of standard .NET types which are also standard Python types.  These types have a standard
		/// set of extension types which are shared between all runtimes.
		/// </summary>
		private static Dictionary<Type, ExtensionTypeInfo> MakeSystemTypes()
		{
			var res = new Dictionary<Type, ExtensionTypeInfo>();
			
			// Native CLR types
			//res[typeof(object)] = new ExtensionTypeInfo(typeof(ObjectOps), "object");
			//res[typeof(string)] = new ExtensionTypeInfo(typeof(StringOps), "str");
			//res[typeof(int)] = new ExtensionTypeInfo(typeof(Int32Ops), "int");
			//res[typeof(bool)] = new ExtensionTypeInfo(typeof(BoolOps), "bool");
			//res[typeof(double)] = new ExtensionTypeInfo(typeof(DoubleOps), "float");
			res[typeof(ValueType)] = new ExtensionTypeInfo(typeof(ValueType), "ValueType");   // just hiding it's methods in the inheritance hierarchy
			
			// MS.Math types
			//res[typeof(BigInteger)] = new ExtensionTypeInfo(typeof(BigIntegerOps), "long");
			//res[typeof(Complex)] = new ExtensionTypeInfo(typeof(ComplexOps), "complex");
			
			// DLR types
			//res[typeof(DynamicNull)] = new ExtensionTypeInfo(typeof(NoneTypeOps), "NoneType");
			//res[typeof(BaseSymbolDictionary)] = new ExtensionTypeInfo(typeof(DictionaryOps), "dict");
			//res[typeof(IAttributesCollection)] = new ExtensionTypeInfo(typeof(DictionaryOps), "dict");
			//res[typeof(IDictionary<object, object>)] = new ExtensionTypeInfo(typeof(DictionaryOps), "dict");
			//res[typeof(NamespaceTracker)] = new ExtensionTypeInfo(typeof(NamespaceTrackerOps), "namespace#");
			//res[typeof(TypeGroup)] = new ExtensionTypeInfo(typeof(TypeGroupOps), "type-collision");
			//res[typeof(TypeTracker)] = new ExtensionTypeInfo(typeof(TypeTrackerOps), "type-collision");
			
			return res;
		}

		public static bool IsExtendedType(Type t)
		{
			Assert.NotNull(t);
			
			return sys_types.ContainsKey(t);
		}
		
		public static bool IsExpressoType(Type t)
		{
			Assert.NotNull(t);
			
			return sys_types.ContainsKey(t) || t.IsDefined(typeof(ExpressoTypeAttribute), false);
		}
	}
}

