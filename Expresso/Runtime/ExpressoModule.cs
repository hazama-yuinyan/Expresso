using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Dynamic;
using System.Diagnostics;

using Expresso.Builtins;
using Expresso.Runtime.Operations;
using Expresso.Compiler.Meta;

namespace Expresso.Runtime
{
	/// <summary>
	/// Expresso module. Stores classes, functions, and data. A module
	/// is created by importing a file or package from disk.
	/// </summary>
	[ExpressoType("module"), DebuggerTypeProxy(typeof(ExpressoModule.DebugProxy)), DebuggerDisplay("module: {GetName()}")]
	public class ExpressoModule : IDynamicMetaObjectProvider/*, IPythonMembersList*/
	{
		private readonly ModuleDefinition def;
		
		//private Scope _scope;
		
		/*public PythonModule() {
			_dict = new PythonDictionary();
			if (GetType() != typeof(PythonModule) && this is IPythonObject) {
				// we should share the user dict w/ our dict.
				((IPythonObject)this).ReplaceDict(_dict);
			}
		}*/
		
		/// <summary>
		/// Creates a new module backed by a Scope.  Used for creating modules for foreign Scope's.
		/// </summary>
		/*internal PythonModule(PythonContext context, Scope scope) {
			_dict = new PythonDictionary(new ScopeDictionaryStorage(context, scope));
			_scope = scope;
		}*/
		
		/// <summary>
		/// Creates a new module backed by a Scope.  Used for creating modules for Python code.
		/// </summary>
		/*internal PythonModule(PythonDictionary dict, Scope scope) {
			_dict = dict;
			_scope = scope;
		}*/
		
		/// <summary>
		/// Creates a new ExpressoModule with the specified ModuleDefinition.
		/// 
		/// Used for creating modules for builtin modules which don't have any code associated with them.
		/// </summary>
		internal ExpressoModule(ModuleDefinition moduleDef)
		{
			def = moduleDef;
		}

		/*public static ExpressoModule New(CodeContext context, ExpressoType cls, params object[] args)
		{
			ExpressoModule res;
			//if (cls == TypeCache.Module) {
				res = new ExpressoModule();
			} else if (cls.IsSubclassOf(TypeCache.Module)) {
				res = (ExpressoModule)cls.CreateInstance(context);
			} else {
				throw ExpressoOps.InvalidTypeError("{0} is not a subtype of module", cls.Name);
			}
			
			return res;
		}*/
		
		/*[StaticExtensionMethod]
		public static PythonModule New(CodeContext context, ExpressoType cls, [ParamDictionary]PythonDictionary kwDict\u00F8, params object[] args\u00F8)
		{
			return __new__(context, cls, args\u00F8);
		}*/

		public void constructor()
		{
		}

		public void constructor(string name)
		{
			constructor(name, null);
		}
		
		public void constructor(string name, string documentation)
		{
			throw new NotImplementedException();
			//def["__name__"] = name;
			//_dict["__doc__"] = documentation;
		}

		public object LookupMember(string name)
		{
			return def.GetMember(name, true);
		}

		public bool HasMember(string name)
		{
			return def.HasMember(name);
		}
		
		/*public object __getattribute__(CodeContext context, string name)
		{
			PythonTypeSlot slot;
			object res;
			if (GetType() != typeof(PythonModule) &&
			    DynamicHelpers.GetPythonType(this).TryResolveMixedSlot(context, name, out slot) &&
			    slot.TryGetValue(context, this, DynamicHelpers.GetPythonType(this), out res)) {
				return res;
			}
			
			switch (name) {
				// never look in the dict for these...
			case "__dict__": return __dict__;
			case "__class__": return DynamicHelpers.GetPythonType(this);
			}
			
			if (_dict.TryGetValue(name, out res)) {
				return res;
			}
			
			// fall back to object to provide all of our other attributes (e.g. __setattr__, etc...)
			return ObjectOps.__getattribute__(context, this, name);
		}
		
		internal object GetAttributeNoThrow(CodeContext context, string name)
		{
			PythonTypeSlot slot;
			object res;
			if (GetType() != typeof(PythonModule) &&
			    DynamicHelpers.GetPythonType(this).TryResolveMixedSlot(context, name, out slot) &&
			    slot.TryGetValue(context, this, DynamicHelpers.GetPythonType(this), out res)) {
				return res;
			}
			
			switch (name) {
				// never look in the dict for these...
			case "__dict__": return __dict__;
			case "__class__": return DynamicHelpers.GetPythonType(this);
			}
			
			if (_dict.TryGetValue(name, out res)) {
				return res;
			} else if (DynamicHelpers.GetPythonType(this).TryGetNonCustomMember(context, this, name, out res)) {
				return res;
			}
			
			return OperationFailed.Value;
		}
		
		public void __setattr__(CodeContext context, string name, object value)
		{
			PythonTypeSlot slot;
			if (GetType() != typeof(PythonModule) &&
			    DynamicHelpers.GetPythonType(this).TryResolveMixedSlot(context, name, out slot) &&
			    slot.TrySetValue(context, this, DynamicHelpers.GetPythonType(this), value)) {
				return;
			}
			
			switch (name) {
			case "__dict__": throw PythonOps.TypeError("readonly attribute");
			case "__class__": throw PythonOps.TypeError("__class__ assignment: only for heap types");
			}
			
			Debug.Assert(value != Uninitialized.Instance);
			
			_dict[name] = value;
		}*/
		
		public string Repr()
		{
			return ToString();
		}
		
		public override string ToString()
		{
			/*object fileObj, nameObj;
			if(!def.GetMemberOffset("__file__", out fileObj)){
				fileObj = null;
			}
			if (!_dict._storage.TryGetName(out nameObj)) {
				nameObj = null;
			}
			
			string file = fileObj as string;
			string name = nameObj as string ?? "?";
			
			if (file == null) {
				return String.Format("<module '{0}' (built-in)>", name);
			}
			return String.Format("<module '{0}' from '{1}'>", name, file);*/
			return string.Format("[module: {0}]", GetName());
		}
		
		internal ModuleDefinition Definition{
			get {
				return def;
			}
		}
		
		/*[SpecialName, PropertyMethod]
		public PythonDictionary Get__dict__() {
			return _dict;
		}
		
		[SpecialName, PropertyMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void Set__dict__(object value) {
			throw PythonOps.TypeError("readonly attribute");
		}
		
		[SpecialName, PropertyMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void Delete__dict__() {
			throw PythonOps.TypeError("readonly attribute");
		}
		
		internal Scope Scope {
			get {
				if (_scope == null) {
					Interlocked.CompareExchange(ref _scope, new Scope(_dict), null);
				}
				
				return _scope;
			}
		}*/
		
		#region IDynamicMetaObjectProvider Members
		[ExpressoHidden] // needs to be public so that we can override it.
		public DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return null;
			//return new MetaModule(this, parameter);
		}
		#endregion
		
		/*class MetaModule : MetaPythonObject, IPythonGetable {
			public MetaModule(PythonModule module, Expression self)
			: base(self, BindingRestrictions.Empty, module) {
			}
			
			public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
				return GetMemberWorker(binder, PythonContext.GetCodeContextMO(binder));
			}
			
			#region IPythonGetable Members
			public DynamicMetaObject GetMember(PythonGetMemberBinder member, DynamicMetaObject codeContext) {
				return GetMemberWorker(member, codeContext);
			}
			#endregion
			
			private DynamicMetaObject GetMemberWorker(DynamicMetaObjectBinder binder, DynamicMetaObject codeContext) {
				string name = GetGetMemberName(binder);
				var tmp = Expression.Variable(typeof(object), "res");                
				
				return new DynamicMetaObject(
					Expression.Block(
					new[] { tmp },
				Expression.Condition(
					Expression.Call(
					typeof(PythonOps).GetMethod("ModuleTryGetMember"),
					PythonContext.GetCodeContext(binder),
					Utils.Convert(Expression, typeof(PythonModule)),
					Expression.Constant(name),
					tmp
					),
					tmp,
					Expression.Convert(GetMemberFallback(this, binder, codeContext).Expression, typeof(object))
					)
				),
					BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
					);
			}
			
			public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder action, DynamicMetaObject[] args) {
				return BindingHelpers.GenericInvokeMember(action, null, this, args);
			}
			
			public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
				Debug.Assert(value.Value != Uninitialized.Instance);
				
				return new DynamicMetaObject(
					Expression.Block(
					Expression.Call(
					Utils.Convert(Expression, typeof(PythonModule)),
					typeof(PythonModule).GetMethod("__setattr__"),
					PythonContext.GetCodeContext(binder),
					Expression.Constant(binder.Name),
					Expression.Convert(value.Expression, typeof(object))
					),
					Expression.Convert(value.Expression, typeof(object))
					),
					BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
					);
			}
			
			public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
				return new DynamicMetaObject(
					Expression.Call(
					Utils.Convert(Expression, typeof(PythonModule)),
					typeof(PythonModule).GetMethod("__delattr__"),
					PythonContext.GetCodeContext(binder),
					Expression.Constant(binder.Name)
					),
					BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
					);
			}
			
			public override IEnumerable<string> GetDynamicMemberNames() {
				foreach (object o in ((PythonModule)Value).__dict__.Keys) {
					string str = o as string;
					if (str != null) {
						yield return str;
					}
				}
			}
		}*/
		
		/*internal string GetFile()
		{
			object res;
			if (def.TryGetValue("__file__", out res)) {
				return res as string;
			}
			return null;
		}*/
		
		public string GetName()
		{
			return def.Name;
		}

		/*#region IPythonMembersList Members
		IList<object> IPythonMembersList.GetMemberNames(CodeContext context) {
			return new List<object>(__dict__.Keys);
		}
		#endregion
		
		#region IMembersList Members
		IList<string> IMembersList.GetMemberNames() {
			List<string> res = new List<string>(__dict__.Keys.Count);
			foreach (object o in __dict__.Keys) {
				string strKey = o as string;
				if (strKey != null) {
					res.Add(strKey);
				}
			}
			
			return res;
		}
		#endregion*/
		
		internal class DebugProxy
		{
			private readonly ExpressoModule module;
			
			public DebugProxy(ExpressoModule inputModule)
			{
				module = inputModule;
			}
			
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public List<ExpressoObjectDebugView> Members{
				get {
					var res = new List<ExpressoObjectDebugView>();
					foreach(var v in module.def){
						res.Add(new ExpressoObjectDebugView(v.Key, v.Value));
					}
					return res;
				}
			}

			public string GetName()
			{
				return module.GetName();
			}
		}
	}
}

