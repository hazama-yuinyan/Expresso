using System;
using System.Collections.Generic;

using Expresso.Ast;
using Expresso.Interpreter;


namespace Expresso.Builtins
{
	public abstract class BaseDefinition
	{
		public string Name{
			get; protected set;
		}
		
		public List<BaseDefinition> Bases{
			get; protected set;
		}
		
		public object[] Members{
			get; set;
		}

		public Dictionary<string, int> PrivateMembers{
			get; protected set;
		}
		
		public Dictionary<string, int> PublicMembers{
			get; protected set;
		}

		/// <summary>
		/// この型で定義されているメンバーのメモリーオフセットを算出する。アクセスできないメンバーだった場合は例外を投げる。
		/// Calculates one of the members of this type. If it is not accessible, then an exception is thrown.
		/// </summary>
		public virtual int GetMemberOffset(object subscription, bool isInsideClass)
		{
			if(subscription is Identifier){
				Identifier mem_name = (Identifier)subscription;
				if(mem_name.Offset == -1){
					int offset;
					if(!PublicMembers.TryGetValue(mem_name.Name, out offset)){
						if(!isInsideClass)
							throw new EvalException(mem_name.Name + " is not accessible.");
						
						if(!PrivateMembers.TryGetValue(mem_name.Name, out offset))
							throw new EvalException(Name + " doesn't have the member called " + mem_name.Name);
					}
					mem_name.Offset = offset;
				}
				return mem_name.Offset;
			}else{
				throw new EvalException("Invalid use of accessor!");
			}
		}
	}

	public class ModuleDefinition : BaseDefinition
	{
		public Dictionary<string, int> InternalMembers{
			get{
				return base.PrivateMembers;
			}
			private set{
				base.PrivateMembers = value;
			}
		}
		
		public Dictionary<string, int> ExportedMembers{
			get{
				return base.PublicMembers;
			}
			private set{
				base.PublicMembers = value;
			}
		}
		
		public ModuleDefinition(string name, Dictionary<string, int> internalMembers, Dictionary<string, int> exportedMembers)
		{
			Name = name;
			Bases = null;
			InternalMembers = internalMembers;
			ExportedMembers = exportedMembers;
		}
		
		public int GetNumberOfMembers()
		{
			return InternalMembers.Count + ExportedMembers.Count;
		}
	}
	
	public class InterfaceDefinition : BaseDefinition
	{
		
	}
	
	public class ClassDefinition : BaseDefinition
	{
		public ClassDefinition(string name, Dictionary<string, int> privateMembers, Dictionary<string, int> publicMembers)
		{
			Name = name;
			PrivateMembers = privateMembers;
			PublicMembers = publicMembers;
		}
		
		public int GetNumberOfMembers()
		{
			return PrivateMembers.Count + PublicMembers.Count;
		}
	}
	
	public class StructDefinition : BaseDefinition
	{
		public StructDefinition(string name, Dictionary<string, int> privateMembers, Dictionary<string, int> publicMembers)
		{
			Name = name;
			
		}
	}
}

