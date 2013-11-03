using System;

namespace Expresso.Runtime
{
	/// <summary>
	/// 実行可能なコード断片をあらわす。
	/// Represents a piece of code. This can reference either a CompiledCode
	/// object or a Function. The user can explicitly call FunctionCode by
	/// passing it into eval.
	/// </summary>
	[ExpressoType("Code")]
	public class FunctionCode
	{
		[ExpressoHidden]
		public Delegate Target;                                     // the current target for the function.  This can change based upon adaptive compilation, recursion enforcement, and tracing.
		Delegate normal_delegate;                           // the normal delegate - this can be a compiled or interpreted delegate.
		Ast.ScopeStatement lambda;               			// the original DLR lambda that contains the code
		readonly int local_count;                           // the number of local variables in the code
		readonly int arg_count;                             // cached locally because it's used during calls w/ defaults
		
		// debugging/tracing support
		//LambdaExpression _tracingLambda;                    // the transformed lambda used for tracing/debugging
		//Delegate _tracingDelegate;                          // the delegate used for tracing/debugging, if one has been created. This can be interpreted or compiled.

		/// <summary>
		/// Constructor used to create a FunctionCode for code that's been serialized to disk.  
		/// 
		/// Code constructed this way cannot be interpreted or debugged using sys.settrace/sys.setprofile.
		/// 
		/// Function codes created this way do support recursion enforcement and are therefore registered in the global function code registry.
		/// </summary>
		internal FunctionCode(ExpressoContext context, Delegate code, Ast.ScopeStatement scope, string documentation, int localCount)
		{
			normal_delegate = code;
			lambda = scope;
			arg_count = CalculateArgumentCount();
			//_initialDoc = documentation;
			
			// need to take this lock to ensure sys.settrace/sys.setprofile is not actively changing
			/*lock (_CodeCreateAndUpdateDelegateLock) {
				Target = AddRecursionCheck(context, code);
			}
			
			RegisterFunctionCode(context);*/
		}
		
		/// <summary>
		/// Constructor to create a FunctionCode at runtime.
		/// 
		/// Code constructed this way supports both being interpreted and debugged.  When necessary the code will
		/// be re-compiled or re-interpreted for that specific purpose.
		/// 
		/// Function codes created this way do support recursion enforcement and are therefore registered in the global function code registry.
		/// 
		/// the initial delegate provided here should NOT be the actual code.  It should always be a delegate which updates our Target lazily.
		/// </summary>
		internal FunctionCode(ExpressoContext context, Delegate initialDelegate, Ast.ScopeStatement scope, string documentation)
		{
			lambda = scope;
			Target = initialDelegate;
			//_initialDoc = documentation;
			local_count = scope.Variables == null ? 0 : scope.Variables.Count;
			arg_count = CalculateArgumentCount();
			
			//RegisterFunctionCode(context);
		}

		int CalculateArgumentCount()
		{
			int arg_cnt = lambda.ArgCount;
			//FunctionAttributes flags = Flags;
			//if ((flags & FunctionAttributes.ArgumentList) != 0) argCnt--;
			//if ((flags & FunctionAttributes.KeywordDictionary) != 0) argCnt--;
			return arg_cnt;
		}

		#region Internal API Surface
		/*internal LambdaExpression Code {
			get {
				return _lambda.GetLambda();
			}
		}*/
		
		internal Ast.ScopeStatement ExpressoCode {
			get {
				return (Ast.ScopeStatement)lambda;
			}
		}
		
		/*internal object Call(CodeContext context) {
			if (co_freevars != PythonTuple.EMPTY) {
				throw PythonOps.TypeError("cannot exec code object that contains free variables: {0}", co_freevars.__repr__(context));
			}
			
			if (Target == null) {
				UpdateDelegate(context.LanguageContext, true);
			}
			
			Func<CodeContext, CodeContext> classTarget = Target as Func<CodeContext, CodeContext>;
			if (classTarget != null) {
				return classTarget(context);
			}
			
			Func<CodeContext, FunctionCode, object> moduleCode = Target as Func<CodeContext, FunctionCode, object>;
			if (moduleCode != null) {
				return moduleCode(context, this);
			}
			
			Func<FunctionCode, object> optimizedModuleCode = Target as Func<FunctionCode, object>;
			if (optimizedModuleCode != null) {
				return optimizedModuleCode(this);
			}
			
			var func = new PythonFunction(context, this, null, ArrayUtils.EmptyObjects, new MutableTuple<object>());
			CallSite<Func<CallSite, CodeContext, PythonFunction, object>> site = PythonContext.GetContext(context).FunctionCallSite;
			return site.Target(site, context, func);
		}*/
		#endregion
	}
}

