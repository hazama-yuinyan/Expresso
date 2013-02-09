using System;
using System.Runtime.CompilerServices;
using System.Threading;

using Expresso.Utils;
using Expresso.Runtime.Meta;

namespace Expresso.Runtime.Types
{
	/// <summary>
	/// インスタンスを生成するためのヘルパークラス。
	/// Base class for helper which creates instances. We have two derived types: One for user
	/// defined types which prepends the type before calling, and one for .NET types which
	/// doesn't prepend the type.
	/// </summary>
	abstract class InstanceCreator
	{
		private readonly ExpressoType type;
		
		protected InstanceCreator(ExpressoType inputType)
		{
			Assert.NotNull(inputType);
			
			type = inputType;
		}
		
		public static InstanceCreator Make(ExpressoType type)
		{
			if(type.IsSystemType)
				return new SystemInstanceCreator(type);
			
			return new UserInstanceCreator(type);
		}
		
		protected ExpressoType Type{
			get {
				return type;
			}
		}
		
		internal abstract object CreateInstance(CodeContext context);
		internal abstract object CreateInstance(CodeContext context, object arg0);
		internal abstract object CreateInstance(CodeContext context, object arg0, object arg1);
		internal abstract object CreateInstance(CodeContext context, object arg0, object arg1, object arg2);
		internal abstract object CreateInstance(CodeContext context, params object[] args);
		//internal abstract object CreateInstance(CodeContext context, object[] args, string[] names);
	}
	
	class UserInstanceCreator : InstanceCreator
	{
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object[], object>> ctor_site;
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object>> ctor_site0;
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object, object>> ctor_site1;
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object, object, object>> ctor_site2;
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object, object, object, object>> ctor_site3;
		
		public UserInstanceCreator(ExpressoType type)
		: base(type) {
		}
		
		internal override object CreateInstance(CodeContext context)
		{
			if(ctor_site0 == null){
				Interlocked.CompareExchange(
					ref ctor_site0,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object>>.Create(
						context.LanguageContext.InvokeOne
					),
					null
				);
			}
			
			return ctor_site0.Target(ctor_site0, context, Type.Ctor, Type);
		}
		
		internal override object CreateInstance(CodeContext context, object arg0)
		{
			if(ctor_site1 == null){
				Interlocked.CompareExchange(
					ref ctor_site1,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object, object>>.Create(
						context.LanguageContext.Invoke(
							new CallSignature(2)
						)
					),
					null
				);
			}
			
			return ctor_site1.Target(ctor_site1, context, Type.Ctor, Type, arg0);
		}
		
		internal override object CreateInstance(CodeContext context, object arg0, object arg1)
		{
			if(ctor_site2 == null){
				Interlocked.CompareExchange(
					ref ctor_site2,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object, object, object>>.Create(
						context.LanguageContext.Invoke(
							new CallSignature(3)
						)
					),
					null
				);
			}
			
			return ctor_site2.Target(ctor_site2, context, Type.Ctor, Type, arg0, arg1);
		}
		
		internal override object CreateInstance(CodeContext context, object arg0, object arg1, object arg2)
		{
			if(ctor_site3 == null){
				Interlocked.CompareExchange(
					ref ctor_site3,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object, object, object, object>>.Create(
						context.LanguageContext.Invoke(
							new CallSignature(4)
						)
					),
					null
				);
			}
			
			return ctor_site3.Target(ctor_site3, context, Type.Ctor, Type, arg0, arg1, arg2);
		}
		
		internal override object CreateInstance(CodeContext context, params object[] args)
		{
			if(ctor_site == null){
				Interlocked.CompareExchange(
					ref ctor_site,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, ExpressoType, object[], object>>.Create(
						context.LanguageContext.Invoke(
							new CallSignature(
								new ArgumentInfo(ArgumentType.Simple),
								new ArgumentInfo(ArgumentType.List)
							)
						)
					),
					null
				);
			}
			
			return ctor_site.Target(ctor_site, context, Type.Ctor, Type, args);
		}
		
		/*internal override object CreateInstance(CodeContext context, object[] args, string[] names)
		{
			return PythonOps.CallWithKeywordArgs(context, Type.Ctor, ArrayUtils.Insert(Type, args), names);
		}*/
	}
	
	class SystemInstanceCreator : InstanceCreator
	{
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object[], object>> ctor_site;
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object>> ctor_site0;
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object>> ctor_site1;
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, object>> ctor_site2;
		private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, object, object>> ctor_site3;
		
		public SystemInstanceCreator(ExpressoType type)
		: base(type) {
		}
		
		internal override object CreateInstance(CodeContext context)
		{
			if(ctor_site0 == null){
				Interlocked.CompareExchange(
					ref ctor_site0,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, object>>.Create(
						context.LanguageContext.InvokeNone
					),
					null
				);
			}
			
			return ctor_site0.Target(ctor_site0, context, Type.Ctor);
		}
		
		internal override object CreateInstance(CodeContext context, object arg0)
		{
			if(ctor_site1 == null){
				Interlocked.CompareExchange(
					ref ctor_site1,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object>>.Create(
						context.LanguageContext.Invoke(
							new CallSignature(1)
						)
					),
					null
				);
			}
			
			return ctor_site1.Target(ctor_site1, context, Type.Ctor, arg0);
		}
		
		internal override object CreateInstance(CodeContext context, object arg0, object arg1)
		{
			if(ctor_site2 == null){
				Interlocked.CompareExchange(
					ref ctor_site2,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, object>>.Create(
						context.LanguageContext.Invoke(
							new CallSignature(2)
						)
					),
					null
				);
			}
			
			return ctor_site2.Target(ctor_site2, context, Type.Ctor, arg0, arg1);
		}
		
		internal override object CreateInstance(CodeContext context, object arg0, object arg1, object arg2)
		{
			if(ctor_site3 == null){
				Interlocked.CompareExchange(
					ref ctor_site3,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, object, object>>.Create(
						context.LanguageContext.Invoke(
							new CallSignature(3)
						)
					),
					null
				);
			}
			
			return ctor_site3.Target(ctor_site3, context, Type.Ctor, arg0, arg1, arg2);
		}
		
		internal override object CreateInstance(CodeContext context, params object[] args)
		{
			if(ctor_site == null){
				Interlocked.CompareExchange(
					ref ctor_site,
					CallSite<Func<CallSite, CodeContext, BuiltinFunction, object[], object>>.Create(
						context.LanguageContext.Invoke(
							new CallSignature(
								new ArgumentInfo(ArgumentType.List)
							)
						)
					),
					null
				);
			}
			
			return ctor_site.Target(ctor_site, context, Type.Ctor, args);
		}
		
		/*internal override object CreateInstance(CodeContext context, object[] args, string[] names)
		{
			return PythonOps.CallWithKeywordArgs(context, Type.Ctor, args, names);
		}*/
	}
}

