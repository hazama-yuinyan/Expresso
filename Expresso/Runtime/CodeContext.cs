using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Expresso.Runtime
{
	/// <summary>
	/// Captures and flows the state of executing code from the generated 
	/// Expresso code into the Expresso runtime.
	/// </summary>    
	/*[DebuggerTypeProxy(typeof(CodeContext.DebugProxy)), DebuggerDisplay("module: {ModuleName}", Type="module")]*/
	public sealed class CodeContext
	{
		private readonly ModuleContext mod_context;
		private readonly Dictionary<object, object> dict;
		
		/// <summary>
		/// Creates a new CodeContext which is backed by the specified dictionary object.
		/// </summary>
		public CodeContext(Dictionary<object, object> inputDict, ModuleContext moduleContext)
		{
			if(inputDict == null) throw new ArgumentNullException("inputDict");
			if(moduleContext == null) throw new ArgumentNullException("moduleContext");
			dict = inputDict;
			mod_context = moduleContext;
		}
		
		#region Public APIs
		/// <summary>
		/// Gets the module state for top-level code.
		/// </summary>   
		public ModuleContext ModuleContext{
			get {
				return mod_context;
			}
		}
		
		/// <summary>
		/// Gets the ExpressoContext which created the CodeContext.
		/// </summary>
		public ExpressoContext LanguageContext{
			get {
				return mod_context.Context;
			}
		}
		#endregion
		
		#region Internal Helpers
		/// <summary>
		/// Gets the dictionary for the global variables from the ModuleContext.
		/// </summary>
		internal Dictionary<object, object> GlobalDict{
			get {
				return mod_context.Globals;
			}
		}
		
		/// <summary>
		/// True if this global context should display CLR members on shared types (for example .ToString on int/bool/etc...)
		/// 
		/// False if these attributes should be hidden.
		/// </summary>
		/*internal bool ShowCls {
			get {
				return ModuleContext.ShowCls;
			}
			set {
				ModuleContext.ShowCls = value;
			}
		}*/
		
		/// <summary>
		/// Attempts to lookup the provided name in this scope or any outer scope.
		/// </summary>
		internal bool TryLookupName(string name, out object value) {
			string strName = name;
			if (dict.TryGetValue(strName, out value)) {
				return true;
			}
			
			return mod_context.Globals.TryGetValue(strName, out value);
		}
		
		/// <summary>
		/// Looks up a global variable. If the variable is not defined in the
		/// global scope then built-ins is consulted.
		/// </summary>
		/*internal bool TryLookupBuiltin(string name, out object value) {
			object builtins;
			if (!GlobalDict.TryGetValue("__builtins__", out builtins)) {
				value = null;
				return false;
			}
			
			PythonModule builtinsScope = builtins as PythonModule;
			if (builtinsScope != null && builtinsScope.__dict__.TryGetValue(name, out value)) {
				return true;
			}
			
			PythonDictionary dict = builtins as PythonDictionary;
			if (dict != null && dict.TryGetValue(name, out value)) {
				return true;
			}
			
			value = null;
			return false;
		}*/
		
		/// <summary>
		/// Gets the dictionary used for storage of local variables.
		/// </summary>
		internal Dictionary<object, object> Dict{
			get {
				return dict;
			}
		}
		
		/// <summary>
		/// Attempts to lookup the variable in the local scope.
		/// </summary>
		internal bool TryGetVariable(string name, out object value)
		{
			return Dict.TryGetValue(name, out value);
		}
		
		/// <summary>
		/// Removes a variable from the local scope.
		/// </summary>
		internal bool TryRemoveVariable(string name)
		{
			return Dict.Remove(name);
		}
		
		/// <summary>
		/// Sets a variable in the local scope.
		/// </summary>
		internal void SetVariable(string name, object value)
		{
			Dict.Add(name, value);
		}
		
		/// <summary>
		/// Gets a variable from the global scope.
		/// </summary>
		internal bool TryGetGlobalVariable(string name, out object res)
		{
			return GlobalDict.TryGetValue(name, out res);
		}
		
		
		/// <summary>
		/// Sets a variable in the global scope.
		/// </summary>
		internal void SetGlobalVariable(string name, object value)
		{
			GlobalDict.Add(name, value);
		}
		
		/// <summary>
		/// Removes a variable from the global scope.
		/// </summary>
		internal bool TryRemoveGlobalVariable(string name)
		{
			return GlobalDict.Remove(name);
		}
		
		internal bool IsTopLevel{
			get {
				return Dict != ModuleContext.Globals;
			}
		}
		
		/// <summary>
		/// Returns the dictionary associated with __builtins__ if one is
		/// set or null if it's not available.  If __builtins__ is a module
		/// the module's dictionary is returned.
		/// </summary>
		/*internal PythonDictionary GetBuiltinsDict() {
			object builtins;
			if (GlobalDict._storage.TryGetBuiltins(out builtins)) {
				PythonModule builtinsScope = builtins as PythonModule;
				if (builtinsScope != null) {
					return builtinsScope.__dict__;
				}
				
				return builtins as PythonDictionary;
			}
			
			return null;
		}*/
		
		internal ExpressoModule Module{
			get {
				return mod_context.Module;
			}
		}
		
		internal string ModuleName{
			get {
				return Module.GetName();
			}
		}
		
		internal class DebugProxy {
			private readonly CodeContext context;
			
			public DebugProxy(CodeContext codeContext) {
				context = codeContext;
			}
			
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public ExpressoModule Members {
				get {
					return context.Module;
				}
			}
		}
		#endregion
	}
}

