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
		CLASS
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
		public static readonly TypeAnnotation VariadicType = new TypeAnnotation(TYPES.VAR);
		public static readonly TypeAnnotation VoidType = new TypeAnnotation(TYPES.UNDEF);
	}
	
	/// <summary>
	/// Represents a class.
	/// </summary>
	public class ExpressoClass
	{
		public class ClassDefinition
		{
			public string Name{
				get; private set;
			}

			public Dictionary<string, int> PrivateMembers{
				get; private set;
			}

			public Dictionary<string, int> PublicMembers{
				get; private set;
			}

			public object[] Members{
				get; internal set;
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

		public class ExpressoObj : IEnumerable<object>, IEnumerable
		{
			private ClassDefinition definition;

			private object[] members;

			/// <summary>
			/// このインスタンスのクラス名。
			/// The class name of this object.
			/// </summary>
			public string ClassName{
				get{return definition.Name;}
			}

			public ExpressoObj(ClassDefinition definition)
			{
				this.definition = definition;
				int num_mems = definition.GetNumberOfMembers();
				this.members = new object[num_mems];
				for(int i = 0; i < definition.Members.Length; ++i)
					this.members[i] = definition.Members[i];
			}

			public override bool Equals (object obj)
			{
				var x = obj as ExpressoObj;
				if(x == null) return false;

				return ClassName == x.ClassName && members[0].Equals(x.members[0]);
			}

			public override int GetHashCode ()
			{
				return definition.GetHashCode() ^ members.GetHashCode();
			}

			public override string ToString ()
			{
				return string.Format("[ExpressoObj: ClassName={0}]", ClassName);
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
				}else if(members[0] is Dictionary<object, object>){
					var dictionary = (Dictionary<object, object>)members[0];
					foreach(var tmp in dictionary)
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
				if(subscription is Identifier){
					Identifier mem_name = (Identifier)subscription;
					var public_mems = definition.PublicMembers;
					if(mem_name.Offset == -1){
						int offset;
						if(!public_mems.TryGetValue(mem_name.Name, out offset)){
							if(!isInsideClass)
								throw new EvalException(mem_name.Name + " is not accessible.");

							var private_mems = definition.PrivateMembers;
							if(!private_mems.TryGetValue(mem_name.Name, out offset))
								throw new EvalException(mem_name.Name + " is not defined in the class \"" + ClassName + "\"");
						}
						mem_name.Offset = offset;
					}
					return members[mem_name.Offset];
				}else{
					throw new EvalException("Invalid use of accessor!");
				}
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
				var public_mems = definition.PublicMembers;
				var private_mems = definition.PrivateMembers;
				if(target.Offset == -1){
					int offset;
					if(!public_mems.TryGetValue(target.Name, out offset)){
						if(isInsideClass){
							if(!private_mems.TryGetValue(target.Name, out offset))
								throw new EvalException(ClassName + " doesn't have the member called " + target.Name);
						}else{
							throw new EvalException(target.Name + " is not accessible!");
						}
					}

					target.Offset = offset;
				}

				members[target.Offset] = val;
			}
		}

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
}

