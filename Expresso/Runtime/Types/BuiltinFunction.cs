using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Runtime;
using Expresso.Runtime.Operations;
using Expresso.Utils;

namespace Expresso.Runtime.Types
{
	/// <summary>
	/// Represents a set of attributes that different functions can have.
	/// </summary>
	[Flags]
	public enum FunctionType
	{
		/// <summary>No flags have been set </summary>
		None = 0x0000,
		/// <summary>This is a function w/ no instance pointer </summary>
		Function = 0x0001,
		/// <summary>This is a method that requires an instance</summary>
		Method = 0x0002,
		/// <summary>Built-in functions can encapsulate both methods and functions, in which case both bits are set</summary>
		FunctionMethodMask = 0x0003,
		/// <summary>True if the function/method should be visible from pure-Expresso code</summary>
		AlwaysVisible = 0x0004,
		/// <summary>This is a constructor</summary>
		Constructor = 0x0020,
		/// <summary>
		/// This method represents a binary operator method for a CLS overloaded operator method.
		/// 
		/// Being a binary operator causes the following special behaviors to kick in:
		///     A failed binding at call time returns NotImplemented instead of raising an exception
		///     A reversed operator will automatically be created if:
		///         1. The parameters are both of the instance type
		///         2. The parameters are in reversed order (other, this)
		///         
		/// This enables simple .NET operator methods to be mapped into the Python semantics.
		/// </summary>
		BinaryOperator = 0x0040,
	}

	/// <summary>
	/// BuiltinFunction represents any standard CLR function exposed to Expresso.
	/// This is used for both methods on standard Expresso types such as list or tuple
	/// and for methods from arbitrary .NET assemblies.
	/// 
	/// All calls are made through the optimizedTarget which is created lazily.
	/// </summary>    
	[ExpressoType("builtinFunction")]
	public class BuiltinFunction : ICodeFormattable, IDelegateConvertible
	{
		internal readonly BuiltinFunctionData data;		//information describing the BuiltinFunction
		internal readonly object instance;				//the bound instance or null if unbound
		private static readonly object no_instance = new object();

		#region StaticFactories
		/// <summary>
		/// Creates a new builtin function for a static .NET function. This is used for module methods
		/// and well-known op_new methods.
		/// </summary>
		internal static BuiltinFunction MakeFunction(string name, MethodBase[] infos, Type declaringType)
		{
#if DEBUG
			foreach(MethodBase mi in infos)
				Debug.Assert(!mi.ContainsGenericParameters);
#endif
			
			return new BuiltinFunction(name, infos, declaringType, FunctionType.AlwaysVisible | FunctionType.Function);
		}
		
		/// <summary>
		/// Creates a built-in function for a .NET method declared on a type.
		/// </summary>
		internal static BuiltinFunction MakeMethod(string name, MethodBase[] infos, Type declaringType, FunctionType ft)
		{
			foreach(MethodBase mi in infos){
				if (mi.ContainsGenericParameters)
					return new GenericBuiltinFunction(name, infos, declaringType, ft);
			}
			
			return new BuiltinFunction(name, infos, declaringType, ft);
		}
		
		internal virtual BuiltinFunction BindToInstance(object instance)
		{
			return new BuiltinFunction(instance, data);
		}
		#endregion

		#region Constructors
		internal BuiltinFunction(string name, MethodBase[] originalTargets, Type declaringType, FunctionType functionType)
		{
			Assert.NotNull(name);
			Assert.NotNull(declaringType);
			Assert.NotNullItems(originalTargets);
			
			data = new BuiltinFunctionData(name, originalTargets, declaringType, functionType);
			instance = no_instance;
		}
		
		/// <summary>
		/// Creates a bound built-in function. The instance may be null for built-in functions
		/// accessed for null.
		/// </summary>
		internal BuiltinFunction(object boundTarget, BuiltinFunctionData funcData)
		{
			Assert.NotNull(data);

			data = funcData;
			instance = boundTarget;
		}
		#endregion

		#region Internal API Surface
		internal void AddMethod(MethodInfo mi)
		{
			data.AddMethod(mi);
		}
		
		internal bool TestData(object otherData)
		{
			return data == otherData;
		}
		
		internal bool IsUnbound{
			get {
				return instance == no_instance;
			}
		}
		
		internal string Name{
			get {
				return data.Name;
			}
			set {
				data.Name = value;
			}
		}

		/// <summary>
		/// Returns a BuiltinFunction bound to the provided type arguments.  Returns null if the binding
		/// cannot be performed.
		/// </summary>
		internal BuiltinFunction MakeGenericMethod(Type[] types)
		{
			TypeList tl = new TypeList(types);
			
			// check for cached method first...
			BuiltinFunction bf;
			if(data.BoundGenerics != null){
				lock(data.BoundGenerics){
					if(data.BoundGenerics.TryGetValue(tl, out bf))
						return bf;
				}
			}
			
			// Search for generic targets with the correct arity (number of type parameters).
			// Compatible targets must be MethodInfos by definition (constructors never take
			// type arguments).
			List<MethodBase> targets = new List<MethodBase>(Targets.Count);
			foreach(MethodBase mb in Targets){
				MethodInfo mi = mb as MethodInfo;
				if(mi == null)
					continue;

				if(mi.ContainsGenericParameters && mi.GetGenericArguments().Length == types.Length)
					targets.Add(mi.MakeGenericMethod(types));
			}
			
			if(targets.Count == 0)
				return null;
			
			// Build a new ReflectedMethod that will contain targets with bound type arguments & cache it.
			bf = new BuiltinFunction(Name, targets.ToArray(), DeclaringType, FunctionType.Method);
			
			EnsureBoundGenericDict();
			
			lock(data.BoundGenerics)
				data.BoundGenerics[tl] = bf;
			
			return bf;
		}

		public Type DeclaringType{
			[ExpressoHidden]
			get {
				return data.DeclaringType;
			}
		}
		
		/// <summary>
		/// Gets the target methods that we'll be calling.  
		/// </summary>
		public IList<MethodBase> Targets{
			[ExpressoHidden]
			get {
				return data.Targets;                
			}
		}
		
		/// <summary>
		/// True if the method should be visible to non-CLS opt-in callers
		/// </summary>
		internal /*override*/ bool IsAlwaysVisible{
			get {
				return (data.Type & FunctionType.AlwaysVisible) != 0;
			}
		}
		#endregion

		#region ICodeFormattable members
		public string Repr(CodeContext context)
		{
			if(IsUnbound)
				return string.Format("<built-in function{0}>", Name);

			return string.Format("<built-in method {0} of type {1}>", Name, ExpressoOps.GetExpressoTypeName(instance));
		}
		#endregion

		#region Private members
		private void EnsureBoundGenericDict()
		{
			if(data.BoundGenerics == null){
				Interlocked.CompareExchange<Dictionary<TypeList, BuiltinFunction>>(
					ref data.BoundGenerics,
					new Dictionary<TypeList, BuiltinFunction>(1),
					null);
			}
		}

		internal class TypeList
		{
			private Type[] types;
			
			public TypeList(Type[] inputTypes)
			{
				Assert.NotNull(inputTypes);
				types = inputTypes;
			}
			
			public override bool Equals(object obj)
			{
				TypeList tl = obj as TypeList;
				if (tl == null || types.Length != tl.types.Length) return false;
				
				for(int i = 0; i < types.Length; ++i){
					if (types[i] != tl.types[i]) return false;
				}
				return true;
			}
			
			public override int GetHashCode()
			{
				int hc = 6551;
				foreach(Type t in types)
					hc = (hc << 5) ^ t.GetHashCode();

				return hc;
			}
		}
		#endregion

		#region IDelegateConvertible members
		Delegate IDelegateConvertible.ConvertToDelegate(Type type)
		{
			return null;
		}
		#endregion

		#region BuiltinFunctionData
		internal sealed class BuiltinFunctionData
		{
			public string Name;
			public MethodBase[] Targets;
			public readonly Type DeclaringType;
			public FunctionType Type;
			public Dictionary<TypeList, BuiltinFunction> BoundGenerics;
			public Dictionary<BuiltinFunction.TypeList, BuiltinFunction> OverloadDictionary;
			//public Dictionary<CallKey, OptimizingInfo> FastCalls;
			
			public BuiltinFunctionData(string name, MethodBase[] targets, Type declType, FunctionType functionType)
			{
				Name = name;
				Targets = targets;
				DeclaringType = declType;
				Type = functionType;
			}
			
			internal void AddMethod(MethodBase info)
			{
				Assert.NotNull(info);
				
				MethodBase[] ni = new MethodBase[Targets.Length + 1];
				Targets.CopyTo(ni, 0);
				ni[Targets.Length] = info;
				Targets = ni;
			}
		}
		#endregion
	}

	/// <summary>
	/// A custom built-in function which supports indexing 
	/// </summary>
	public class GenericBuiltinFunction : BuiltinFunction
	{
		internal GenericBuiltinFunction(string name, MethodBase[] originalTargets, Type declaringType, FunctionType functionType)
		: base(name, originalTargets, declaringType, functionType)
		{
		}
		
		public BuiltinFunction this[ExpressoTuple tuple] {
			get {
				return this[tuple.Items];
			}
		}
		
		internal GenericBuiltinFunction(object instance, BuiltinFunctionData data) : base(instance, data)
		{
		}
		
		
		internal override BuiltinFunction BindToInstance(object instance)
		{
			return new GenericBuiltinFunction(instance, data);
		}
		
		/// <summary>
		/// Use indexing on generic methods to provide a new reflected method with targets bound with
		/// the supplied type arguments.
		/// </summary>
		public BuiltinFunction this[params object[] key]{
			get {
				// Retrieve the list of type arguments from the index.
				Type[] types = new Type[key.Length];
				for(int i = 0; i < types.Length; ++i)
					types[i] = Converter.ConvertToType(key[i]);
				
				BuiltinFunction res = MakeGenericMethod(types);
				if(res == null){
					bool hasGenerics = false;
					foreach(MethodBase mb in Targets){
						MethodInfo mi = mb as MethodInfo;
						if(mi != null && mi.ContainsGenericParameters){
							hasGenerics = true;
						}
					}
					
					throw ExpressoOps.InvalidTypeError(hasGenerics ? string.Format("bad type args to this generic method {0}", Name) :
					                                   string.Format("{0} is not a generic method and is unsubscriptable", Name));
				}
				
				if(IsUnbound)
					return res;
				
				return new BuiltinFunction(instance, res.data);
			}
		}
		
		internal /*override*/ bool IsOnlyGeneric {
			get {
				foreach(MethodBase mb in Targets){
					if(!mb.IsGenericMethod || !mb.ContainsGenericParameters)
						return false;
				}
				
				return true;
			}
		}
	}
}

