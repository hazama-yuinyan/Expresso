using System;
using System.Collections;
using System.Collections.Generic;

using Expresso.Ast;
using Expresso.Interpreter;
using Expresso.Compiler.Meta;
using Expresso.Runtime.Operations;

namespace Expresso.Runtime
{
	/// <summary>
	/// ユーザー定義のExpressoの型のインスタンスをあらわす。
	/// Represents an instance of user-defined Expresso types.
	/// </summary>
	public class ExpressoObj : IEnumerable<object>, IEnumerable
	{
		private BaseDefinition definition;
		
		private object[] members;
		
		/// <summary>
		/// このインスタンスの型名。
		/// The type name of this object.
		/// </summary>
		public string Name{
			get{return definition.Name;}
		}

		public ExpressoObj(ModuleDefinition definition)
		{
			this.definition = definition;
			this.members = definition.Members;
		}
		
		public ExpressoObj(ClassDefinition definition)
		{
			this.definition = definition;
			int num_mems = definition.GetNumberOfMembers();
			this.members = new object[num_mems];
			for(int i = 0; i < definition.Members.Length; ++i)
				this.members[i] = definition.Members[i];
		}

		public ExpressoObj(StructDefinition definition)
		{
			this.definition = definition;
			int num_mems = definition.GetNumberOfMembers();
			this.members = new object[num_mems];
			for(int i = 0; i < definition.Members.Length; ++i)
				this.members[i] = definition.Members[i];
		}
		
		public override bool Equals(object obj)
		{
			var x = obj as ExpressoObj;
			if(x == null) return false;
			
			return Name == x.Name && members.Equals(x.members);
		}
		
		public override int GetHashCode()
		{
			return definition.GetHashCode() ^ members.GetHashCode();
		}
		
		public override string ToString()
		{
			return string.Format("[ExpressoObj: TypeName={0}]", Name);
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		
		public IEnumerator<object> GetEnumerator()
		{
			if(members[0] is IEnumerable<object>){
				var enumerable = (IEnumerable<object>)members[0];
				foreach(var tmp in enumerable)
					yield return tmp;
			}else
				throw ExpressoOps.InvalidTypeError("Can not evaluate the object to an iterable.");
		}
		
		/// <summary>
		/// このインスタンスのメンバーにアクセスする。
		/// Accesses one of the members of this instance.
		/// </summary>
		public object AccessMember(object subscription, bool isInsideClass)
		{
			int offset = definition.GetMemberOffset(subscription, isInsideClass);
			return members[offset];
		}

		public object AccessMemberWithName(string name, bool isInsideClass)
		{
			return definition.GetMember(name, isInsideClass);
		}

		public object GetMember(int index)
		{
			return members[index];
		}
		
		/// <summary>
		/// Assigns an object to a specified field.
		/// </summary>
		public void Assign(Identifier target, object val, bool isInsideClass)
		{
			int offset = definition.GetMemberOffset(target, isInsideClass);
			members[offset] = val;
		}

		/// <summary>
		/// Expressoのオブジェクトを生成する。
		/// Creates a new instance of an Expresso object.
		/// </summary>
		/// <returns>
		/// The instance.
		/// </returns>
		/// <param name='definition'>
		/// The target type definition to be constructed.
		/// </param>
		/// <param name='args'>
		/// Arguments that passed to the constructor.
		/// </param>
		/// <param name='varStore'>
		/// The environment.
		/// </param>
		public static ExpressoObj CreateInstance(CodeContext context, BaseDefinition definition, Expression[] args)
		{
			ExpressoObj new_inst = null;
			if(definition is ClassDefinition)
				new_inst = new ExpressoObj((ClassDefinition)definition);
			else if(definition is StructDefinition)
				new_inst = new ExpressoObj((StructDefinition)definition);
			else if(definition is InterfaceDefinition)
				throw ExpressoOps.InvalidTypeError("Can not instantiate an interface!");
			else
				throw ExpressoOps.InvalidTypeError("Unknown definition.");
			
			var constructor = new_inst.AccessMember(new Identifier("constructor", null), true) as FunctionDefinition;
			/*if(constructor != null){
				var value_this = new Constant{ValType = ObjectTypes.INSTANCE, Value = new_inst};	//thisの値としてインスタンスを追加する
				args.Insert(0, value_this);
				var call_ctor = new Call{
					Function = constructor,
					Arguments = args,
					Reference = null
				};
				call_ctor.Run(varStore);
			}*/
			return new_inst;
		}
	}
}

