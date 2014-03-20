using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Expresso.Compiler.Meta;
using Expresso.Runtime;
using Expresso.Runtime.Operations;
using Expresso.Runtime.Binding;

namespace Expresso.Runtime.Types
{
	/// <summary>
	/// Expressoの型に関するメタデータなどを保持する。
	/// Expresso type.
	/// </summary>
	[ExpressoType("Type")]
	public class ExpressoType : ICodeFormattable
	{
		BaseDefinition type_def;				// the metadata representing members of this type
		Type underlying_system_type;            // the underlying CLI system type for this type
		ExpressoTypeAttributes attrs;			// attributes of the type
		ExpressoContext exs_context;            // the context the type was created from, or null for system types.

		// commonly calculatable
		//List<PythonType> _resolutionOrder;          // the search order for methods in the type
		BuiltinFunction ctor;                   // the built-in function which allocates an instance - a .NET ctor
		InstanceCreator instance_ctor;

		static readonly Dictionary<Type, ExpressoType> exs_types = new Dictionary<Type, ExpressoType>();

		public string Name{
			get{return type_def.Name;}
		}

		public ExpressoType()
		{
		}

		/// <summary>
		/// Creates a new ExpressoType object which is backed by the specified .NET type for
		/// storage. The type is considered a system type which can not be modified
		/// by the user.
		/// </summary>
		/// <param name="underlyingSystemType"></param>
		internal ExpressoType(Type underlyingSystemType)
		{
			underlying_system_type = underlyingSystemType;
			
			InitializeSystemType();
		}

		internal BuiltinFunction Ctor{
			get {
				EnsureConstructor();
				
				return ctor;
			}
		}

		#region Internal API
		/// <summary>
		/// Gets the underlying system type that is backing this type. All instances of this
		/// type are an instance of the underlying system type.
		/// </summary>
		internal Type UnderlyingSystemType{
			get {
				return underlying_system_type;
			}
		}

		internal BaseDefinition Def{
			get{
				return type_def;
			}
		}

		/// <summary>
		/// True if the type is a system type.  A system type is a type which represents an
		/// underlying .NET type and not a subtype of one of these types.
		/// </summary>
		internal bool IsSystemType{
			get {
				return (attrs & ExpressoTypeAttributes.SystemType) != 0;
			}
			set {
				if (value)
					attrs |= ExpressoTypeAttributes.SystemType;
				else
					attrs &= ~ExpressoTypeAttributes.SystemType;
			}
		}

		internal bool HasSystemCtor{
			get {
				return (attrs & ExpressoTypeAttributes.SystemCtor) != 0;
			}
		}
		
		internal void SetConstructor(BuiltinFunction constructor)
		{
			ctor = constructor;
		}
		
		internal bool IsExpressoType{
			get {
				return (attrs & ExpressoTypeAttributes.IsExpressoType) != 0;
			}
			set {
				if(value)
					attrs |= ExpressoTypeAttributes.IsExpressoType;
				else
					attrs &= ~ExpressoTypeAttributes.IsExpressoType;
			}
		}

		internal ExpressoContext ExpressoContext{
			get {
				return exs_context;
			}
		}
		
		internal ExpressoContext Context{
			get {
				return exs_context;// ?? DefaultContext.DefaultPythonContext;
			}
		}

		/// <summary>
		/// Gets the dynamic type that corresponds with the provided static type. 
		/// 
		/// Returns null if no type is available.  TODO: In the future this will
		/// always return a ExpressoType created by the DLR.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static ExpressoType GetExpressoType(Type type)
		{
			ExpressoType res;
			
			if(!exs_types.TryGetValue(type, out res)){
				lock(exs_types){
					if(!exs_types.TryGetValue(type, out res)){
						res = new ExpressoType(type);
						
						exs_types.Add(type, res);
					}
				}
			}
			
			return res;
		}
		#endregion

		#region ICodeFormattable members
		public string Repr(CodeContext context)
		{
			return "<type>";
		}
		#endregion

		#region System type initialization
		/// <summary>
		/// Initializes a ExpressoType that represents a standard .NET type. The same .NET type
		/// can be shared with the Expresso type system. For example object, string, int,
		/// etc... are all the same types.  
		/// </summary>
		void InitializeSystemType()
		{
			IsSystemType = true;
			IsExpressoType = ExpressoBinder.IsExpressoType(underlying_system_type);
			type_def = SystemTypeDefinition.MakeSystemType(underlying_system_type);
		}
		
		/// <summary>
		/// Creates a op_new method for the type. If the type defines interesting constructors
		/// then the op_new method will call that. Otherwise if it has only a single argless
		/// </summary>
		void AddSystemConstructors()
		{
			if(typeof(Delegate).IsAssignableFrom(underlying_system_type)){
				SetConstructor(
					BuiltinFunction.MakeFunction(
						underlying_system_type.Name,
						new[] { typeof(DelegateOps).GetMethod("op_new") },
						underlying_system_type
					)
				);
			}else if(!underlying_system_type.IsAbstract){
				BuiltinFunction reflectedCtors = GetConstructors();
				if(reflectedCtors == null)
					return; // no ctors, no op_new
				
				SetConstructor(reflectedCtors);
			}
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		BuiltinFunction GetConstructors()
		{
			Type type = underlying_system_type;
			string name = Name;
			
			return ExpressoTypeOps.GetConstructorFunction(type, name);
		}
		
		void EnsureConstructor()
		{
			if(ctor == null){
				AddSystemConstructors();
				if(ctor == null)
					throw ExpressoOps.MakeInvalidTypeError(string.Format("{0} does not define any public constructors.",
					                                                 underlying_system_type.FullName));
			}
		}
		
		/*void EnsureInstanceCtor()
		{
			if(instance_ctor == null)
				instance_ctor = InstanceCreator.Make(this);
		}*/
		#endregion

		#region Private implementation details
		[Flags]
		enum ExpressoTypeAttributes
		{
			None = 0x00,
			Immutable = 0x01,
			SystemType = 0x02,
			IsExpressoType = 0x04,
			WeakReferencable = 0x08,
			HasDictionary = 0x10,
			
			/// <summary>
			/// The type has a ctor which does not accept PythonTypes.  This is used
			/// for user defined types which implement __clrtype__
			/// </summary>
			SystemCtor    = 0x20
		}
		#endregion
	}
}

