using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Expresso.Ast;
using Expresso.Interpreter;
using Expresso.Runtime.Operations;


namespace Expresso.Compiler.Meta
{
	/// <summary>
	/// Base class for type definitions in Expresso. It contains the type name, accessibilities for each member
	/// and members themselves.
	/// </summary>
	public abstract class BaseDefinition : IEnumerable, IEnumerable<KeyValuePair<string, object>>
	{
		public string Name{
			get; protected set;
		}
		
		public List<BaseDefinition> Bases{
			get; protected set;
		}
		
		public object[] Members{
			get; protected set;
		}

		public Dictionary<string, int> PrivateMembers{
			get; protected set;
		}
		
		public Dictionary<string, int> PublicMembers{
			get; protected set;
		}

		public BaseDefinition(string name, Dictionary<string, int> privateMembers, Dictionary<string, int> publicMembers,
		                      object[] members, List<BaseDefinition> bases = null)
		{
			Name = name;
			PrivateMembers = privateMembers;
			PublicMembers = publicMembers;
			Members = members;
			Bases = bases;
		}

		/// <summary>
		/// この型で定義されているメンバーのメモリーオフセットを算出する。アクセスできないメンバーだった場合は例外を投げる。
		/// Calculates the offset for one of the members of this type. If it is not accessible, then an exception is thrown.
		/// </summary>
		public virtual int GetMemberOffset(object subscription, bool isInsideClass)
		{
			if(subscription is Identifier){
				Identifier mem_name = (Identifier)subscription;
				if(mem_name.Offset == -1){
					int offset;
					if(!PublicMembers.TryGetValue(mem_name.Name, out offset)){
						if(!isInsideClass)
							throw ExpressoOps.ReferenceError("{0} is not accessible.", mem_name.Name);
						
						if(!PrivateMembers.TryGetValue(mem_name.Name, out offset))
							throw ExpressoOps.InvalidTypeError("{0} doesn't have the member called {1}", Name, mem_name.Name);
					}
					//mem_name.Offset = offset;
				}
				return mem_name.Offset;
			}else{
				throw ExpressoOps.RuntimeError("Invalid use of accessor!");
			}
		}

		public virtual object GetMember(string name, bool accessedFromInsideClass)
		{
			int offset = -1;
			if(!PublicMembers.TryGetValue(name, out offset)){
				if(!accessedFromInsideClass)
					throw ExpressoOps.ReferenceError("{0} is not accessible.", name);

				if(!PrivateMembers.TryGetValue(name, out offset))
					throw ExpressoOps.InvalidTypeError("{0} doesn't have the member called {1}", Name, name);
			}
			return Members[offset];
		}

		public virtual bool HasMember(string name)
		{
			int offset = -1;
			if(PublicMembers.TryGetValue(name, out offset))
				return true;
			else
				return PrivateMembers.TryGetValue(name, out offset);
		}

		#region IEnumerable members
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region IEnumerable<KeyValuePair<string, object>> members
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return EnumerateMembers();
		}
		#endregion

		public IEnumerator<KeyValuePair<string, object>> EnumerateMembers()
		{
			var offset_map = PrivateMembers.Concat(PublicMembers);
			var ordered_offset_map = offset_map.OrderBy((value) => value.Value);
			var member_map = ordered_offset_map.Join(Members.Select((obj, i) => new Tuple<int, object>(i, obj)),
			                                         (outer) => outer.Value, (inner) => inner.Item1,
			                                         (outer, inner) => new KeyValuePair<string, object>(outer.Key, inner.Item2));

			return member_map.GetEnumerator();
		}
	}

    /// <summary>
    /// Represents a module in Expresso.
    /// </summary>
	public sealed class ModuleDefinition : BaseDefinition
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
		
		public ModuleDefinition(string name, Dictionary<string, int> internalMembers, Dictionary<string, int> exportedMembers,
		                        object[] members)
			: base(name, internalMembers, exportedMembers, members, null)
		{
		}
		
		public int GetNumberOfMembers()
		{
			return InternalMembers.Count + ExportedMembers.Count;
		}
	}
	
    /// <summary>
    /// Represents an interface.
    /// </summary>
	public sealed class InterfaceDefinition : BaseDefinition
	{
		public InterfaceDefinition(string name, Dictionary<string, int> publicMembers, object[] members, List<BaseDefinition> bases)
			: base(name, null, publicMembers, members, bases)
		{
		}
	}
	
    /// <summary>
    /// Represents a class.
    /// </summary>
	public sealed class ClassDefinition : BaseDefinition
	{
		public ClassDefinition(string name, Dictionary<string, int> privateMembers, Dictionary<string, int> publicMembers,
		                       object[] members, List<BaseDefinition> bases)
			: base(name, privateMembers, publicMembers, members, bases)
		{
		}
		
		public int GetNumberOfMembers()
		{
			return PrivateMembers.Count + PublicMembers.Count;
		}
	}
	
    /// <summary>
    /// Represents a struct.
    /// </summary>
	public sealed class StructDefinition : BaseDefinition
	{
		public StructDefinition(string name, Dictionary<string, int> privateMembers, Dictionary<string, int> publicMembers,
		                        object[] members, List<BaseDefinition> bases)
			: base(name, privateMembers, publicMembers, members, bases)
		{
		}

		public int GetNumberOfMembers()
		{
			return PrivateMembers.Count + PublicMembers.Count;
		}
	}
}

