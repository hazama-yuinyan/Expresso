using System;
using System.Collections.Generic;
using System.Reflection;

using Expresso.Runtime.Types;

namespace Expresso.Runtime.Operations
{
	internal static class ExpressoTypeOps
	{
		internal static BuiltinFunction GetConstructorFunction(Type type, string name)
		{
			List<MethodBase> methods = new List<MethodBase>();
			bool hasDefaultConstructor = false;
			
			foreach(ConstructorInfo ci in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)){
				if(ci.IsPublic){
					if(ci.GetParameters().Length == 0){
						hasDefaultConstructor = true;
					}
					
					methods.Add(ci);
				}
			}
			
			/*if(type.IsValueType && !hasDefaultConstructor && type != typeof(void)){
				try {
					methods.Add(typeof(ScriptingRuntimeHelpers).GetMethod("CreateInstance", Type.EmptyTypes).MakeGenericMethod(type));
				} catch (BadImageFormatException) {
					// certain types (e.g. ArgIterator) won't survive the above call.
					// we won't let you create instances of these types.
				}
			}*/
			
			if(methods.Count > 0)
				return BuiltinFunction.MakeFunction(name, methods.ToArray(), type);
			
			return null;
		}
	}
}

