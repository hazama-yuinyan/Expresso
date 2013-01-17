using System;
using System.Collections.Generic;
using System.Threading;

using Expresso.Runtime.Binding;
using Expresso.Runtime.Meta;

namespace Expresso.Runtime
{
	public sealed class ExpressoContext
	{
		internal const string ExpressoDisplayName = "Expresso 1.0.0";
		internal const string ExpressoNames = "Expresso;exs";
		internal const string ExpressoFileExtensions = ".exs";
		
		private static readonly Guid ExpressoLanguageGuid = new Guid("03ed4b80-d10b-442f-ad9a-47dae85b2051");

		private readonly CodeContext default_context;
		private readonly Dictionary<string, ExpressoModule> loaded_modules = new Dictionary<string, ExpressoModule>();

		private ExpressoInvokeBinder invoke_no_args, invoke_one_arg;
		private Dictionary<CallSignature, ExpressoInvokeBinder> invoke_binders;

		public ExpressoContext()
		{
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
				if (!invoke_binders.TryGetValue(signature, out res)) {
					invoke_binders[signature] = res = new ExpressoInvokeBinder(this, signature);
				}
				
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
	}
}

