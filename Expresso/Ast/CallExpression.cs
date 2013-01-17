using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// 関数呼び出し。
	/// Reperesents a function call.
    /// </summary>
    public class Call : Expression
    {
		private readonly Expression target;
		private readonly Expression[] args;

        /// <summary>
        /// 呼び出す対象。
		/// The target function to be called.
		/// It can be null if it is a call to a method, since methods are implemetend in the way of
		/// objects having function objects and they are resolved at runtime.
        /// </summary>
        public Expression Target{
			get{return target;}
		}

        /// <summary>
        /// 与える実引数リスト。
		/// The argument list to be supplied to the call.
        /// </summary>
        public Expression[] Arguments{
			get{return args;}
		}

		//public MethodContainer MethodInfo{get{return method_info;}}

		//private MethodContainer method_info = null;

		public Call(Expression targetExpr, Expression[] arguments)
		{
			target = targetExpr;
			args = arguments;
		}

        public override NodeType Type
        {
            get { return NodeType.Call; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Call;

            if (x == null) return false;

            if (this.args.Length != x.args.Length) return false;

            for (int i = 0; i < this.args.Length; ++i)
            {
                if (!this.args[i].Equals(x.args[i])) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return this.target.GetHashCode() ^ this.args.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			var child = new VariableStore{Parent = varStore};
			FunctionDeclaration fn;
			bool this_registered = false;
			if(Reference != null){
				if(method_info == null){
					var obj = Reference.Run(varStore);
					var method = obj as MethodContainer;
					if(method == null)
						throw ExpressoOps.InvalidTypeError("Not callable: {0}", obj.ToString());

					if(method.Inst != null){
						if(Arguments.Count == 0 || Arguments[0] != this_value)	//実引数リストにthisを追加しておく
							Arguments.Insert(0, this_value);
					}

					method_info = method;	//毎回リファレンスを辿るのは遅いと思われるので、キャッシュしておく
				}

				fn = method_info.Method;
				if(!fn.IsStatic && method_info.Inst != null){	//このメソッド呼び出しのthisオブジェクトを登録する
					child.Add(0, method_info.Inst);
					this_registered = true;
				}
			}else{
    	        fn = Function;		//Functionがセットされている場合は、現在のモジュールを暗黙のthis参照として追加する
				if(!fn.IsStatic){
					child.Add(0, varStore.Get(0));
					this_registered = true;
					if(Arguments.Count == 0 || Arguments[0] != this_value)
						Arguments.Insert(0, this_value);
				}
			}

			for(int i = this_registered ? 1 : 0; i < fn.Parameters.Count; ++i){	//実引数をローカル変数として変数テーブルに追加する
				var param = fn.Parameters[i];
				child.Add(param.Offset, (i < Arguments.Count) ? Arguments[i].Run(varStore) : param.Option.Run(varStore));
			}

			var local_vars = fn.LocalVariables;
			if(local_vars.Any()){					//Checking for its emptiness
				foreach(var local in local_vars)	//関数内で定義されているローカル変数を予め初期化しておく
					child.Add(local.Offset, ImplementationHelpers.GetDefaultValueFor(local.ParamType.ObjType));
			}

			return fn.Run(child);
        }

		internal Tuple<FunctionDefinition, bool> ResolveCallTarget()
		{
			FunctionDefinition fn;
			bool this_registered = false;
			if(target is Identifier){
				var ident_target = (Identifier)target;
				if(ident_target.IsResolved){
					fn = ident_target;
					var method = obj as MethodContainer;
					if(method == null)
						throw ExpressoOps.InvalidTypeError("Not callable: {0}", obj.ToString());
					
					if(method.Inst != null){
						if(Arguments.Count == 0 || Arguments[0] != this_value)	//実引数リストにthisを追加しておく
							Arguments.Insert(0, this_value);
					}
					
					method_info = method;	//毎回リファレンスを辿るのは遅いと思われるので、キャッシュしておく
				}
				
				fn = method_info.Method;
				if(!fn.IsStatic && method_info.Inst != null){	//このメソッド呼び出しのthisオブジェクトを登録する
					store.Add(0, method_info.Inst);
					this_registered = true;
				}
			}else{
				fn = Function;		//Functionがセットされている場合は、現在のモジュールを暗黙のthis参照として追加する
				if(!fn.IsStatic){
					store.Add(0, parent.Get(0));
					this_registered = true;
					if(Arguments.Count == 0 || Arguments[0] != this_value)
						Arguments.Insert(0, this_value);
				}
			}

			return new Tuple<FunctionDefinition, bool>(fn, this_registered);
		}*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				target.Walk(walker);
				foreach(var arg in args)
					arg.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return string.Format("[Call for {0} with ({1})]", target, args);
		}
    }
}
