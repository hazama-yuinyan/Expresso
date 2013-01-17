using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Expresso.Utils;

namespace Expresso.Runtime.Meta
{
	/// <summary>
	/// Richly represents the signature of a callsite.
	/// </summary>
	public struct CallSignature : IEquatable<CallSignature>
	{
		// TODO: invariant _infos != null ==> _argumentCount == _infos.Length
		
		/// <summary>
		/// Array of additional meta information about the arguments, such as named arguments.
		/// Null for a simple signature that's just an expression list. eg: foo(a*b,c,d)
		/// </summary>
		private readonly ArgumentInfo[] infos;
		
		/// <summary>
		/// Number of arguments in the signature.
		/// </summary>
		private readonly int arg_count;
		
		/// <summary>
		/// All arguments are unnamed and matched by position. 
		/// </summary>
		public bool IsSimple{
			get { return infos == null; }
		}
		
		public int ArgumentCount{
			get {
				Debug.Assert(infos == null || infos.Length == arg_count);
				return arg_count;
			}
		}
		
		#region Construction
		public CallSignature(CallSignature signature)
		{
			infos = signature.GetArgumentInfos();
			arg_count = signature.arg_count;
		}
		
		public CallSignature(int argumentCount)
		{
			ContractUtils.Requires(argumentCount >= 0, "argumentCount");
			arg_count = argumentCount;
			infos = null;
		}
		
		public CallSignature(params ArgumentInfo[] argInfos)
		{
			bool simple = true;
			
			if(argInfos != null){
				arg_count = argInfos.Length;
				for(int i = 0; i < argInfos.Length; ++i){
					if(argInfos[i].Kind != ArgumentType.Simple){
						simple = false;
						break;
					}
				}
			}else{
				arg_count = 0;
			}
			
			infos = (!simple) ? argInfos : null;
		}
		
		public CallSignature(params ArgumentType[] kinds)
		{
			bool simple = true;
			
			if(kinds != null){
				arg_count = kinds.Length;
				for(int i = 0; i < kinds.Length; ++i){
					if(kinds[i] != ArgumentType.Simple){
						simple = false;
						break;
					}
				}
			}else{
				arg_count = 0;
			}
			
			if(!simple){
				infos = new ArgumentInfo[kinds.Length];
				for(int i = 0; i < kinds.Length; ++i)
					infos[i] = new ArgumentInfo(kinds[i]);
			}else{
				infos = null;
			}
		}
		#endregion
		
		#region IEquatable<CallSignature> Members
		public bool Equals(CallSignature other)
		{
			if(infos == null)
				return other.infos == null && other.arg_count == arg_count;
			else if(other.infos == null)
				return false;
			
			if(infos.Length != other.infos.Length) return false;
			
			for(int i = 0; i < infos.Length; ++i){
				if(!infos[i].Equals(other.infos[i])) return false;
			}
			
			return true;
		}
		#endregion
		
		#region Overrides
		public override bool Equals(object obj)
		{
			return obj is CallSignature && Equals((CallSignature)obj);
		}
		
		public static bool operator ==(CallSignature left, CallSignature right)
		{
			return left.Equals(right);
		}
		
		public static bool operator !=(CallSignature left, CallSignature right)
		{
			return !left.Equals(right);
		}
		
		public override string ToString()
		{
			if(infos == null)
				return "Simple";
			
			StringBuilder sb = new StringBuilder("(");
			for(int i = 0; i < infos.Length; ++i){
				if(i > 0)
					sb.Append(", ");

				sb.Append(infos[i].ToString());
			}
			sb.Append(")");
			return sb.ToString();
		}
		
		public override int GetHashCode()
		{
			int h = 6551;
			if(infos != null){
				foreach(ArgumentInfo info in infos)
					h ^= (h << 5) ^ info.GetHashCode();
			}
			return h;
		}
		#endregion
		
		#region Helpers
		public ArgumentInfo[] GetArgumentInfos()
		{
			return (infos != null) ? ArrayUtils.Copy(infos) : ArrayUtils.MakeDuplicatedArray(ArgumentInfo.Simple, arg_count);
		}
		
		public CallSignature InsertArgument(ArgumentInfo info)
		{
			return InsertArgumentAt(0, info);
		}
		
		public CallSignature InsertArgumentAt(int index, ArgumentInfo info)
		{
			if(this.IsSimple){
				if(info.IsSimple)
					return new CallSignature(arg_count + 1);
				
				return new CallSignature(ArrayUtils.InsertAt(GetArgumentInfos(), index, info));
			}
			
			return new CallSignature(ArrayUtils.InsertAt(infos, index, info));
		}
		
		public CallSignature RemoveFirstArgument()
		{
			return RemoveArgumentAt(0);
		}
		
		public CallSignature RemoveArgumentAt(int index)
		{
			if(arg_count == 0)
				throw new InvalidOperationException();
			
			if(IsSimple)
				return new CallSignature(arg_count - 1);
			
			return new CallSignature(ArrayUtils.RemoveAt(infos, index));
		}
		
		public int IndexOf(ArgumentType kind)
		{
			if(infos == null)
				return (kind == ArgumentType.Simple && arg_count > 0) ? 0 : -1;
			
			for(int i = 0; i < infos.Length; ++i){
				if(infos[i].Kind == kind) 
					return i;
			}
			return -1;
		}
		
		public bool HasDictionaryArgument()
		{
			return IndexOf(ArgumentType.Dictionary) > -1;
		}
		
		public bool HasInstanceArgument()
		{
			return IndexOf(ArgumentType.Instance) > -1;
		}
		
		public bool HasListArgument()
		{
			return IndexOf(ArgumentType.List) > -1;
		}
		
		internal bool HasNamedArgument()
		{
			return IndexOf(ArgumentType.Named) > -1;
		}
		
		/// <summary>
		/// True if the OldCallAction includes an ArgumentInfo of ArgumentKind.Dictionary or ArgumentKind.Named.
		/// </summary>
		public bool HasKeywordArgument()
		{
			if(infos != null){
				foreach(ArgumentInfo info in infos){
					if(info.Kind == ArgumentType.Dictionary || info.Kind == ArgumentType.Named)
						return true;
				}
			}
			return false;
		}
		
		public ArgumentType GetArgumentKind(int index)
		{
			// TODO: Contract.Requires(index >= 0 && index < _argumentCount, "index");
			return infos != null ? infos[index].Kind : ArgumentType.Simple;
		}
		
		public string GetArgumentName(int index)
		{
			ContractUtils.Requires(index >= 0 && index < arg_count);
			return infos != null ? infos[index].Name : null;
		}
		
		/// <summary>
		/// Gets the number of positional arguments the user provided at the call site.
		/// </summary>
		public int GetProvidedPositionalArgumentCount() 
		{
			int result = arg_count;
			
			if(infos != null){
				for(int i = 0; i < infos.Length; ++i){
					ArgumentType kind = infos[i].Kind;
					
					if(kind == ArgumentType.Dictionary || kind == ArgumentType.List || kind == ArgumentType.Named)
						result--;
				}
			}
			
			return result;
		}
		
		public string[] GetArgumentNames()
		{
			if(infos == null)
				return ArrayUtils.EmptyStrings;
			
			List<string> result = new List<string>();
			foreach(ArgumentInfo info in infos){
				if(info.Name != null)
					result.Add(info.Name);
			}
			
			return result.ToArray();
		}
		
		/*public Expression CreateExpression()
		{            
			if(infos == null){
				return Expression.New(
					typeof(CallSignature).GetConstructor(new Type[] { typeof(int) }),
					AstUtils.Constant(ArgumentCount)
				);
			}else{
				Expression[] args = new Expression[infos.Length];
				for(int i = 0; i < args.Length; ++i)
					args[i] = infos[i].CreateExpression();

				return Expression.New(
					typeof(CallSignature).GetConstructor(new Type[] { typeof(ArgumentInfo[]) }), 
					Expression.NewArrayInit(typeof(ArgumentInfo), args)
				);
			}
		}*/
		#endregion
	}
}

