using System;
using System.Dynamic;
using System.Linq.Expressions;

using Expresso.Runtime;
using Expresso.Runtime.Meta;

namespace Expresso.Runtime.Binding
{
	/// <summary>
	/// The Action used for Expresso call sites.  This supports both splatting of position and keyword arguments.
	/// 
	/// When a foreign object is encountered the arguments are expanded into normal position/keyword arguments.
	/// </summary>
	class ExpressoInvokeBinder : DynamicMetaObjectBinder
	{
		private ExpressoContext context;
		private CallSignature call_sig;

		public ExpressoInvokeBinder(ExpressoContext inputContext, CallSignature signature)
		{
			context = inputContext;
			call_sig = signature;
		}

		#region DynamicMetaObjectBinder members
		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			throw new System.NotImplementedException ();
		}

		public override T BindDelegate<T>(System.Runtime.CompilerServices.CallSite<T> site, object[] args)
		{
			return base.BindDelegate(site, args);
		}
		#endregion
	}
}

