using System;
using System.Collections;
using System.Collections.Generic;
using Expresso.Ast;
using Expresso.Interpreter;

namespace Expresso.BuiltIns
{
	public enum TYPES // types
	{
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
		ARRAY,
		CLASS,
		_SUBSCRIPT,
		_METHOD,
		_CASE_DEFAULT
	};
	
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

			public int GetNumberMembers()
			{
				return PrivateMembers.Count + PublicMembers.Count;
			}
		}

		public class ExpressoObj : IEnumerable<object>, IEnumerable
		{
			public TYPES Type{
				get; internal set;
			}

			private ClassDefinition definition;

			private object[] members;

			public string ClassName{
				get{return definition.Name;}
			}

			public ExpressoObj(ClassDefinition definition, TYPES objType = TYPES.CLASS)
			{
				this.definition = definition;
				int num_mems = definition.GetNumberMembers();
				this.members = new object[num_mems];
				for(int i = 0; i < definition.Members.Length; ++i)
					this.members[i] = definition.Members[i];

				this.Type = objType;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			public IEnumerator<object> GetEnumerator()
			{
				if(members[0] is IEnumerable<object>){
					var enumerable = (IEnumerable<object>)members[0];
					return enumerable.GetEnumerator();
				}else
					throw new EvalException("Can not evaluate the object to an iterable.");
			}

			/// <summary>
			/// このインスタンスのメンバーにアクセスする。
			/// Accesses one of the members of this instance.
			/// </summary>
			public object AccessMember(object subscription)
			{
				if(Type == TYPES.DICT){
					object value = null;
					((Dictionary<object, object>)members[0]).TryGetValue(subscription, out value);
					return value;
				}else if(subscription is Identifier){
					Identifier mem_name = (Identifier)subscription;
					var public_mems = definition.PublicMembers;
					int offset;
					if(!public_mems.TryGetValue(mem_name.Name, out offset))
						throw new EvalException(mem_name + " is not accessible.");

					return members[offset];
				}else if(subscription is int){
					int index = (int)subscription;

					switch(Type){
					case TYPES.LIST:
						return ((List<object>)members[0])[index];

					case TYPES.ARRAY:
						return ((object[])members[0])[index];

					case TYPES.TUPLE:
						return ((ExpressoTuple)members[0])[index];

					default:
						throw new EvalException("Can not apply the [] operator on that type of object!");
					}
				}else{
					throw new EvalException("Invalid use of accessor!");
				}
			}

			public object GetMember(int index)
			{
				return members[index];
			}
		}

		static private Dictionary<string, ClassDefinition> classes = new Dictionary<string, ClassDefinition>();

		static public void AddClass(string className, ClassDefinition newClass)
		{
			classes.Add(className, newClass);
		}

		static public ExpressoObj CreateInstance(string className)
		{
			ClassDefinition target_class;
			if(!classes.TryGetValue(className, out target_class))
				throw new EvalException("Can not find the class \"" + className + "\"!");

			var new_instance = new ExpressoObj(target_class);
			return new_instance;
		}
	}
}

