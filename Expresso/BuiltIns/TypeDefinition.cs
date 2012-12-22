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

		public virtual ExpressoObj CreateInstance(string typeName, List<Expression> args, VariableStore varStore)
		{
			return null;
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
		
		public ModuleDefinition(string name, Dictionary<string, int> internalMembers, Dictionary<string, int> exportedMembers,
		                        object[] members)
			: base(name, internalMembers, exportedMembers, members, null)
		{
		}
		
		public int GetNumberOfMembers()
		{
			return InternalMembers.Count + ExportedMembers.Count;
		}

		public override ExpressoObj CreateInstance(string typeName, List<Expression> args, VariableStore varStore)
		{
			int offset = GetMemberOffset(new Identifier(typeName), false);
			object type_def = Members[offset];
			ExpressoObj new_inst = null;
			if(type_def is ClassDefinition)
				new_inst = new ExpressoObj((ClassDefinition)type_def);
			else if(type_def is StructDefinition)
				new_inst = new ExpressoObj((StructDefinition)type_def);
			else if(type_def is InterfaceDefinition)
				throw new EvalException("Can not instantiate an interface!");
			
			var constructor = new_inst.AccessMember(new Identifier("constructor"), true) as Function;
			if(constructor != null){
				var value_this = new Constant{ValType = TYPES.CLASS, Value = new_inst};		//thisの値としてインスタンスを追加する
				args.Insert(0, value_this);
				var call_ctor = new Call{
					Function = constructor,
					Arguments = args,
					Reference = null
				};
				call_ctor.Run(varStore);
			}
			return new_inst;
		}
	}
	
	public class InterfaceDefinition : BaseDefinition
	{
		public InterfaceDefinition(string name, Dictionary<string, int> publicMembers, object[] members, List<BaseDefinition> bases)
			: base(name, null, publicMembers, members, bases)
		{
		}
	}
	
	public class ClassDefinition : BaseDefinition
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
	
	public class StructDefinition : BaseDefinition
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

