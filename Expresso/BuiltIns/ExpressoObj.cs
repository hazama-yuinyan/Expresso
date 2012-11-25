using System;
using System.Collections;
using System.Collections.Generic;
using Expresso.Ast;
using Expresso.Interpreter;

namespace Expresso.BuiltIns
{
	public enum TYPES // types
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
			/// <summary>
			/// オブジェクトの型。
			/// The type of this object.
			/// </summary>
			public TYPES Type{
				get; internal set;
			}

			private ClassDefinition definition;

			private object[] members;

			/// <summary>
			/// このインスタンスのクラス名。
			/// The class name of this object.
			/// </summary>
			public string ClassName{
				get{return definition.Name;}
			}

			public object this[int index]
			{
				get{
					switch(Type){
					case TYPES.LIST:
						return ((List<object>)members[0])[index];

					case TYPES.TUPLE:
						return ((ExpressoTuple)members[0])[index];

					default:
						return null;
					}
				}
			}

			public object this[object key]
			{
				get{
					if(Type == TYPES.DICT){
						object value = null;
						((Dictionary<object, object>)members[0]).TryGetValue(key, out value);
						return value;
					}else
						return null;
				}
			}

			public ExpressoObj(ClassDefinition definition, TYPES objType = TYPES.CLASS)
			{
				this.definition = definition;
				int num_mems = definition.GetNumberOfMembers();
				this.members = new object[num_mems];
				for(int i = 0; i < definition.Members.Length; ++i)
					this.members[i] = definition.Members[i];

				this.Type = objType;
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
				return string.Format("[ExpressoObj: Type={0}, ClassName={1}]", Type, ClassName);
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
				if(Type == TYPES.DICT){
					object value = null;
					((Dictionary<object, object>)members[0]).TryGetValue(subscription, out value);
					return value;
				}else if(subscription is Identifier){
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
				}else if(subscription is int){
					int index = (int)subscription;

					switch(Type){
					case TYPES.LIST:
						return ((List<object>)members[0])[index];

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

			/// <summary>
			/// Assigns an object on a list at a specified index. An error occurs if the instance is not a Expresso's list.
			/// </summary>
			public void Assign(int index, object val)
			{
				if(Type == TYPES.TUPLE)
					throw new EvalException("Can not assign a value on a tuple!");

				if(Type == TYPES.LIST)
					((List<object>)members[0])[index] = val;
				else
					throw new EvalException("Unknown seqeuence type!");
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

			/// <summary>
			/// Assigns an object to a specified key. An exception would be thrown if the instance is not a Expresso's dictionary.
			/// </summary>
			public void Assign(object key, object val)
			{
				if(Type == TYPES.DICT)
					((Dictionary<object, object>)members[0])[key] = val;
				else
					throw new EvalException("Invalid use of the [] operator!");
			}

			/// <summary>
			/// IntegerSequenceを使ってコンテナの一部の要素をコピーした新しいコンテナを生成する。
			/// Do the "slice" operation on the container with an IntegerSequence.
			/// </summary>
			public ExpressoObj Slice(ExpressoIntegerSequence seq)
			{
				ExpressoClass.ExpressoObj result;
				var er = this.GetEnumerator();
				var enumerator = seq.GetEnumerator();

				switch(Type){
				case TYPES.LIST:
				case TYPES.TUPLE:
					var tmp = new List<object>();
					while(er.MoveNext() && enumerator.MoveNext())
						tmp.Add(er.Current);

					result = (Type == TYPES.LIST) ? ExpressoFunctions.MakeList(tmp) : ExpressoFunctions.MakeTuple(tmp);
					break;

				case TYPES.DICT:
					var keys = new List<object>();
					var values = new List<object>();
					while(er.MoveNext() && enumerator.MoveNext()){
						var pair = er.Current as Nullable<KeyValuePair<object, object>>;
						if(pair == null)
							throw new EvalException("Can not evaluate an element to a valid dictionary element.");

						keys.Add(pair.Value.Key);
						values.Add(pair.Value.Value);
					}
					result = ExpressoFunctions.MakeDict(keys, values);
					break;
		
				default:
					throw new EvalException("This object doesn't support slice operation!");
				}

				return result;
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

