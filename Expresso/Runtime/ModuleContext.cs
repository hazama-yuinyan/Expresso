using System;
using System.Collections.Generic;

namespace Expresso.Runtime
{
	/// <summary>
	/// Captures the globals and other state of module code.
	/// </summary>
	public sealed class ModuleContext
	{
		private readonly ExpressoContext exs_context;
		private readonly Dictionary<object, object> globals;
		private readonly CodeContext global_context;
		private readonly ExpressoModule module;
		//private ModuleOptions _features;
		
		/// <summary>
		/// Creates a new ModuleContext which is backed by the specified dictionary.
		/// </summary>
		public ModuleContext(Dictionary<object, object> globalsDict, ExpressoContext creatingContext)
		{
			if(globalsDict == null) throw new ArgumentNullException("globalsDict");
			if(creatingContext == null) throw new ArgumentNullException("creatingContext");
			
			globals = globalsDict;
			exs_context = creatingContext;
			global_context = new CodeContext(globalsDict, this);
			//module = new ExpressoModule(globals);
		}
		
		/// <summary>
		/// Creates a new ModuleContext for the specified module.
		/// </summary>
		public ModuleContext(ExpressoModule inputModule, ExpressoContext creatingContext)
		{
			if(inputModule == null) throw new ArgumentNullException("inputModule");
			if(creatingContext == null) throw new ArgumentNullException("creatingContext");
			
			globals = new Dictionary<object, object>();
			exs_context = creatingContext;
			global_context = new CodeContext(globals, this);
			module = inputModule;
		}
		
		/// <summary>
		/// Gets the dictionary used for the global variables in the module
		/// </summary>
		public Dictionary<object, object> Globals{
			get {
				return globals;
			}
		}
		
		/// <summary>
		/// Gets the language context which created this module.
		/// </summary>
		public ExpressoContext Context{
			get {
				return exs_context;
			}
		}
		
		/// <summary>
		/// Gets the DLR Scope object which is associated with the modules dictionary.
		/// </summary>
		/*public Scope GlobalScope {
			get {
				return _globalScope;
			}
		}*/
		
		/// <summary>
		/// Gets the global CodeContext object which is used for execution of top-level code.
		/// </summary>
		public CodeContext GlobalContext{
			get {
				return global_context;
			}
		}
		
		/// <summary>
		/// Gets the module object which this code is executing in.
		/// 
		/// This module may or may not be published in sys.modules.  For user defined
		/// code typically the module gets published at the start of execution.  But if
		/// this ModuleContext is attached to a Scope, or if we've just created a new
		/// module context for executing code it will not be in sys.modules.
		/// </summary>
		public ExpressoModule Module{
			get {
				return module;
			}
		}
		
		/// <summary>
		/// Gets the features that code has been compiled with in the module.
		/// </summary>
		/*public ModuleOptions Features {
			get {
				return _features;
			}
			set {
				_features = value;
			}
		}
		
		/// <summary>
		/// Gets or sets whether code running in this context should display
		/// CLR members (for example .ToString on objects).
		/// </summary>
		public bool ShowCls {
			get {
				return (_features & ModuleOptions.ShowClsMethods) != 0;
			}
			set {
				Debug.Assert(this != this._pyContext.SharedContext.ModuleContext || !value);
				if (value) {
					_features |= ModuleOptions.ShowClsMethods;
				} else {
					_features &= ~ModuleOptions.ShowClsMethods;
				}
			}
		}*/
	}
}

