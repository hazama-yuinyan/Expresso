using System;
using System.Collections;
using System.Collections.Generic;

using Expresso.Ast;
using Expresso.Interpreter;

namespace Expresso.Builtins
{
	public enum ObjectTypes
	{
		_SUBSCRIPT = -5,
		_CASE_DEFAULT = -4,
		_LABEL_PRIVATE = -3,
		_LABEL_PUBLIC = -2,
		_INFERENCE = -1,
		UNDEF = 0,
		NULL,
		INTEGER,
		BOOL,
		FLOAT,
		RATIONAL,
		BIGINT,
		STRING,
		BYTEARRAY,
		VAR,
		TUPLE,
		LIST,
		DICT,
		EXPRESSION,
		FUNCTION,
		SEQ,
		INSTANCE,
		TYPE_CLASS,
		TYPE_MODULE,
		TYPE_STRUCT
	};

	/// <summary>
	/// Contains type information of Expresso's object.
	/// </summary>
	public class TypeAnnotation : ICloneable
	{
		public ObjectTypes ObjType{get; internal set;}

		public string TypeName{get; internal set;}

		public TypeAnnotation(ObjectTypes type, string name = null)
		{
			ObjType = type;
			TypeName = name;
		}

		public TypeAnnotation Clone()
		{
			return new TypeAnnotation(ObjType, TypeName);
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}

		public override bool Equals(object obj)
		{
			var x = obj as TypeAnnotation;
			if(x == null)
				return false;

			return this.ObjType == x.ObjType && this.TypeName == x.TypeName;
		}

		public override int GetHashCode()
		{
			return ObjType.GetHashCode() ^ TypeName.GetHashCode();
		}

		public override string ToString()
		{
			return (ObjType == ObjectTypes.TYPE_CLASS) ? string.Format("class {0}", TypeName) :
				(ObjType == ObjectTypes.TYPE_MODULE) ? string.Format("module {0}", TypeName) :
					(ObjType == ObjectTypes.TYPE_STRUCT) ? string.Format("struct {0}", TypeName) :
					(TypeName != null) ? string.Format("{0}", TypeName) : string.Format("{0}", ObjType);
		}

		public static readonly TypeAnnotation InferenceType = new TypeAnnotation(ObjectTypes._INFERENCE);
		public static readonly TypeAnnotation VariantType = new TypeAnnotation(ObjectTypes.VAR);
		public static readonly TypeAnnotation VoidType = new TypeAnnotation(ObjectTypes.UNDEF);
	}

	/// <summary>
	/// Represents an instance of Expresso objects.
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
				throw new EvalException("Can not evaluate the object to an iterable.");
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
		public static ExpressoObj CreateInstance(BaseDefinition definition, List<Expression> args, VariableStore varStore)
		{
			ExpressoObj new_inst = null;
			if(definition is ClassDefinition)
				new_inst = new ExpressoObj((ClassDefinition)definition);
			else if(definition is StructDefinition)
				new_inst = new ExpressoObj((StructDefinition)definition);
			else if(definition is InterfaceDefinition)
				throw new EvalException("Can not instantiate an interface!");
			else
				throw new EvalException("Unknown definition.");
			
			var constructor = new_inst.AccessMember(new Identifier("constructor"), true) as Function;
			if(constructor != null){
				var value_this = new Constant{ValType = ObjectTypes.INSTANCE, Value = new_inst};	//thisの値としてインスタンスを追加する
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
	
	public class ExpressoModule
	{
		static private Dictionary<string, ExpressoObj> modules = new Dictionary<string, ExpressoObj>();
		
		static public void AddModule(string name, ExpressoObj moduleInstance)
		{
			modules.Add(name, moduleInstance);
		}

		static public ExpressoObj GetModule(string name)
		{
			ExpressoObj module;
			if(modules.TryGetValue(name, out module))
				return module;
			else
				return null;
		}
	}
}

