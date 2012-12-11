using System;
using System.Collections.Generic;

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
	}
	
	public class InterfaceDefinition : BaseDefinition
	{
		
	}
	
	public class ClassDefinition : BaseDefinition
	{
		public Dictionary<string, int> PrivateMembers{
			get; private set;
		}
		
		public Dictionary<string, int> PublicMembers{
			get; private set;
		}
		
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

