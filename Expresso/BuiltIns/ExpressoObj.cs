using System;
using System.Collections;
using System.Collections.Generic;

using Expresso.Ast;
using Expresso.Interpreter;

namespace Expresso.Builtins
{
	public enum TYPES
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
		CLASS,
		TYPE_CLASS,
		TYPE_MODULE
	};

	/// <summary>
	/// Contains type information of Expresso's object.
	/// </summary>
	public class TypeAnnotation : ICloneable
	{
		public TYPES ObjType{get; internal set;}

		public string TypeName{get; internal set;}

		public TypeAnnotation(TYPES type, string name = null)
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

		public override string ToString()
		{
			return (TypeName != null) ? string.Format("{0}", TypeName) : string.Format("{0}", ObjType);
		}

		public static readonly TypeAnnotation InferenceType = new TypeAnnotation(TYPES._INFERENCE);
		public static readonly TypeAnnotation VariantType = new TypeAnnotation(TYPES.VAR);
		public static readonly TypeAnnotation VoidType = new TypeAnnotation(TYPES.UNDEF);
	}

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
	}
	
	/// <summary>
	/// Represents a class.
	/// </summary>
	public class ExpressoClass
	{
		static private Dictionary<string, ClassDefinition> classes = new Dictionary<string, ClassDefinition>();

		/// <summary>
		/// Dictionary that contains the class definitions.
		/// </summary>
		static public Dictionary<string, ClassDefinition> Classes{get{return ExpressoClass.classes;}}

		/// <summary>
		/// Expressoで定義したクラスを登録する。
		/// Adds a new class definition.
		/// </summary>
		/// <param name='newClass'>
		/// New class.
		/// </param>
		static public void AddClass(ClassDefinition newClass)
		{
			classes.Add(newClass.Name, newClass);
		}

		/// <summary>
		/// Expressoのオブジェクトを生成する。
		/// Creates a new instance of an Expresso object.
		/// </summary>
		/// <returns>
		/// The instance.
		/// </returns>
		/// <param name='className'>
		/// The target class name to be constructed.
		/// </param>
		/// <param name='args'>
		/// Arguments that passed to the constructor.
		/// </param>
		/// <param name='varStore'>
		/// The environment.
		/// </param>
		static public ExpressoObj CreateInstance(string className, List<Expression> args, VariableStore varStore)
		{
			ClassDefinition target_class;
			if(!classes.TryGetValue(className, out target_class))
				throw new EvalException("Can not find the class \"" + className + "\"!");

			var new_instance = new ExpressoObj(target_class);
			var constructor = new_instance.AccessMember(new Identifier("constructor"), true) as Function;
			if(constructor != null){
				var value_this = new Constant{ValType = TYPES.CLASS, Value = new_instance};	//thisの値としてインスタンスを追加する
				args.Insert(0, value_this);
				var call_ctor = new Call{
					Function = constructor,
					Arguments = args,
					Reference = null
				};
				call_ctor.Run(varStore);
			}
			return new_instance;
		}
	}

	public class ExpressoModule
	{
		static private Dictionary<string, ExpressoObj> modules = new Dictionary<string, ExpressoObj>();
		
		/// <summary>
		/// Dictionary that contains module instances.
		/// </summary>
		static public Dictionary<string, ExpressoObj> Classes{get{return ExpressoModule.modules;}}

		static public void AddModule(string name, ExpressoObj moduleInstance)
		{
			modules.Add(name, moduleInstance);
		}
	}
}

