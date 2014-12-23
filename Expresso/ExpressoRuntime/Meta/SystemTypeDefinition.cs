using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Expresso.Runtime.Types;

namespace Expresso.Compiler.Meta
{
	/// <summary>
	/// システム定義型のメタデータを格納する。int, float(double)のほか、list, tupleなどもこの型を使用する。
	/// Describes a system type, which is a standard .NET type.
	/// </summary>
	public class SystemTypeDefinition : BaseDefinition
	{
		internal SystemTypeDefinition(string name, Dictionary<string, int> privateMembers, Dictionary<string, int> publicMembers,
		                              object[] members, List<BaseDefinition> bases)
			: base(name, privateMembers, publicMembers, members, bases)
		{
		}

		internal static SystemTypeDefinition MakeSystemType(Type systemType)
		{
			var privates = new Dictionary<string, int>();
			var publics = new Dictionary<string, int>();
			var members = new List<object>();
			Dictionary<string, int> decl_target = null;
			int offset = 0;

			foreach(var method in systemType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)){
				if(method.IsPublic)
					decl_target = publics;
				else if(method.IsPrivate)
					decl_target = privates;

				decl_target.Add(method.Name, offset++);
				switch(method.MemberType){
				case MemberTypes.Method:
					members.Add(BuiltinFunction.MakeMethod(method.Name, new MethodBase[]{method}, systemType,
					FunctionType.Method));
					break;

				case MemberTypes.Constructor:
					members.Add(BuiltinFunction.MakeFunction(method.Name, new MethodBase[]{method}, systemType));
					break;
				}
			}

			var bases = GetSystemBases(systemType);

			return new SystemTypeDefinition(systemType.Name, privates, publics, members.ToArray(), bases);
		}

		static List<BaseDefinition> GetSystemBases(Type systemType)
		{
			var bases = new List<BaseDefinition>();
			
			if(systemType.BaseType != null){
				Type base_type;
				if(systemType.BaseType == typeof(ValueType)){
					// hide ValueType, it doesn't exist in Python
					base_type = typeof(object);
				}else{
					base_type = systemType.BaseType;
				}
				var exs_type = ExpressoType.GetExpressoType(base_type);
				bases.Add(exs_type.Def);
				
				/*Type cur_type = base_type;
				while(cur_type != null){
					Type new_type;
					if (TryReplaceExtensibleWithBase(cur_type, out new_type))
						mro.Add(DynamicHelpers.GetPythonTypeFromType(new_type));
					else
						mro.Add(DynamicHelpers.GetPythonTypeFromType(cur_type));
					
					cur_type = cur_type.BaseType;
				}*/
				
				//AddSystemInterfaces(mro);
			}else if(systemType.IsInterface){
				// add interfaces to MRO & create bases list
				Type[] interfaces = systemType.GetInterfaces();
				bases.Capacity = interfaces.Length;
				
				for(int i = 0; i < interfaces.Length; ++i){
					Type iface = interfaces[i];
					ExpressoType it = DynamicHelpers.GetExpressoTypeFromType(iface);
					
					bases.Add(it.Def);
				}
			}
			
			//_resolutionOrder = mro;
			return bases;
		}
		
		/*static void AddSystemInterfaces(Type systemType, List<ExpressoType> mro)
		{
			if(systemType.IsArray){
				// include the standard array interfaces in the array MRO.  We pick the
				// non-strongly typed versions which are also in Array.__mro__
				mro.Add(DynamicHelpers.GetExpressoTypeFromType(typeof(IList)));
				mro.Add(DynamicHelpers.GetExpressoTypeFromType(typeof(ICollection)));
				mro.Add(DynamicHelpers.GetExpressoTypeFromType(typeof(IEnumerable)));
				return;
			} 
			
			Type[] interfaces = systemType.GetInterfaces();
			Dictionary<string, Type> methodMap = new Dictionary<string, Type>();
			bool hasExplicitIface = false;
			List<Type> nonCollidingInterfaces = new List<Type>(interfaces);
			
			foreach(Type iface in interfaces){
				InterfaceMapping mapping = systemType.GetInterfaceMap(iface);
				
				// grab all the interface methods which would hide other members
				for(int i = 0; i < mapping.TargetMethods.Length; ++i){
					MethodInfo target = mapping.TargetMethods[i];
					
					if(target == null)
						continue;
					
					if(!target.IsPrivate)
						methodMap[target.Name] = null;
					else
						hasExplicitIface = true;
				}
				
				if(hasExplicitIface){
					for(int i = 0; i < mapping.TargetMethods.Length; i++){
						MethodInfo target = mapping.TargetMethods[i];
						MethodInfo iTarget = mapping.InterfaceMethods[i];
						
						// any methods which aren't explicit are picked up at the appropriate
						// time earlier in the MRO so they can be ignored
						if(target != null && target.IsPrivate){
							hasExplicitIface = true;
							
							Type existing;
							if(methodMap.TryGetValue(iTarget.Name, out existing)){
								if(existing != null){
									// collision, multiple interfaces implement the same name, and
									// we're not hidden by another method.  remove both interfaces, 
									// but leave so future interfaces get removed
									nonCollidingInterfaces.Remove(iface);
									nonCollidingInterfaces.Remove(methodMap[iTarget.Name]);
									break;
								}
							}else{
								// no collisions so far...
								methodMap[iTarget.Name] = iface;
							}
						} 
					}
				}
			}
			
			if(hasExplicitIface){
				// add any non-colliding interfaces into the MRO
				foreach(Type t in nonCollidingInterfaces){
					Debug.Assert(t.IsInterface);
					
					mro.Add(DynamicHelpers.GetExpressoTypeFromType(t));
				}
			}
		}*/
	}
}

