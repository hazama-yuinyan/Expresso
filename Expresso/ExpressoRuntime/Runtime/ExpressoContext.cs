using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using Expresso.Compiler.Meta;
using Expresso.Runtime.Binding;
using Expresso.Runtime.Meta;
using Expresso.Runtime.Types;

namespace Expresso.Runtime
{
	/// <summary>
	/// Expresso言語全体のコンテクスト。
	/// Expresso context.
	/// </summary>
	public sealed class ExpressoContext
	{
		internal const string ExpressoDisplayName = "Expresso 1.3.0";
		internal const string ExpressoNames = "Expresso;exs";
		internal const string ExpressoFileExtensions = ".exs";
		
		//static readonly Guid ExpressoLanguageGuid = new Guid("03ed4b80-d10b-442f-ad9a-47dae85b2051");

		readonly CodeContext default_context;
		readonly Dictionary<string, ExpressoModule> loaded_modules = new Dictionary<string, ExpressoModule>();
		readonly Dictionary<string, Type> builtin_modules = new Dictionary<string, Type>(StringComparer.Ordinal);
		readonly Dictionary<Type, string> builtin_module_names = new Dictionary<Type, string>();

		ExpressoInvokeBinder invoke_no_args, invoke_one_arg;
		Dictionary<CallSignature, ExpressoInvokeBinder> invoke_binders;

		#region Expresso shared call site storage
		/*CallSite<Func<CallSite, CodeContext, object, object>> callsite0;
		CallSite<Func<CallSite, CodeContext, object, object, object>> callsite1;
		CallSite<Func<CallSite, CodeContext, object, object, object, object>> callsite2;
		CallSite<Func<CallSite, CodeContext, object, object, object, object, object>> callsite3;
		CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object>> callsite4;
		CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object>> callsite5;
		CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object, object>> callsite6;*/
		#endregion

		public ExpressoContext()
		{
			builtin_modules = CreateBuiltinTable();
			var default_scope = new Dictionary<object, object>();
			ModuleContext mod_context = new ModuleContext(default_scope, this);
			default_context = mod_context.GlobalContext;
		}

		public void PublishModule(string name, ExpressoModule module)
		{
			loaded_modules.Add(name, module);
		}

		public ExpressoModule GetModule(string name)
		{
			ExpressoModule inst;
			if(loaded_modules.TryGetValue(name, out inst))
				return inst;
			else
				return null;
		}

		/// <summary>
		/// Returns a shared code context for the current ExpressoContext. This shared
		/// context can be used for performing general operations which usually
		/// require a CodeContext.
		/// </summary>
		internal CodeContext SharedContext{
			get {
				return default_context;
			}
		}

		#region Binder factories
		internal ExpressoInvokeBinder Invoke(CallSignature signature)
		{
			if(invoke_binders == null){
				Interlocked.CompareExchange(
					ref invoke_binders,
					new Dictionary<CallSignature, ExpressoInvokeBinder>(),
					null
				);
			}
			
			lock(invoke_binders){
				ExpressoInvokeBinder res;
				if (!invoke_binders.TryGetValue(signature, out res))
					invoke_binders[signature] = res = new ExpressoInvokeBinder(this, signature);
				
				return res;
			}
		}
		
		internal ExpressoInvokeBinder InvokeNone{
			get {
				if(invoke_no_args == null)
					invoke_no_args = Invoke(new CallSignature(0));
				
				return invoke_no_args;
			}
		}
		
		internal ExpressoInvokeBinder InvokeOne{
			get {
				if(invoke_one_arg == null)
					invoke_one_arg = Invoke(new CallSignature(1));
				
				return invoke_one_arg;
			}
		}
		#endregion

        internal object Call(BuiltinFunction func, params object[] args)
        {
            return func.data.Targets;
        }

		Dictionary<string, Type> CreateBuiltinTable()
		{
			var table = new Dictionary<string, Type>();

			LoadBuiltins(table, typeof(ExpressoContext).Assembly);
			return table;
		}

		void LoadBuiltins(Dictionary<string, Type> dict, Assembly assem)
		{
			var attrs = assem.GetCustomAttributes(typeof(ExpressoModuleAttribute), false);
			if(attrs.Length > 0){
				foreach(ExpressoModuleAttribute ema in attrs){
					dict[ema.Name] = ema.Type;
					builtin_module_names[ema.Type] = ema.Name;
				}
			}
		}

		internal Type TryLookupBuiltinModule(string name)
		{
			Type res;
			if(builtin_modules.TryGetValue(name, out res))
				return res;

			return null;
		}
	}
}

