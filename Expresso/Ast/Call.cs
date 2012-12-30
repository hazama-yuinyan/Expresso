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
        /// <summary>
        /// 呼び出す対象。
		/// The target function to be called.
		/// It can be null if it is a call to a method, since methods are implemetend in the way of
		/// objects having function objects and they are resolved at runtime.
        /// </summary>
        public Function Function { get; internal set; }

        /// <summary>
        /// 呼び出す対象の関数名。
		/// The target function name.
        /// </summary>
        public string Name
		{ 
			get{
				if(Function == null)
					return Reference.ToString();
				else
					return this.Function.Name;
			}
		}

        /// <summary>
        /// 与える実引数リスト。
		/// The argument list to be supplied to the call.
        /// </summary>
        public List<Expression> Arguments { get; internal set; }

		/// <summary>
		/// メソッドだった場合のメソッドを指す参照。
		/// Used to reference methods.
		/// </summary>
		public Expression Reference {get; internal set;}

		private MethodContainer method_info = null;

		static private Identifier this_value = new Identifier("this");

        public override NodeType Type
        {
            get { return NodeType.Call; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Call;

            if (x == null) return false;

            if (this.Name != x.Name) return false;

            if (this.Arguments.Count != x.Arguments.Count) return false;

            for (int i = 0; i < this.Arguments.Count; ++i)
            {
                if (!this.Arguments[i].Equals(x.Arguments[i])) return false;
            }

			if(!this.Reference.Equals(x.Reference)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Arguments.GetHashCode() ^ this.Reference.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			var child = new VariableStore{Parent = varStore};
			Function fn;
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

		internal Tuple<Function, bool> ResolveCallTarget(out VariableStore store, VariableStore parent)
		{
			store = new VariableStore{Parent = parent};
			Function fn;
			bool this_registered = false;
			if(Reference != null){
				if(method_info == null){
					var obj = Reference.Run(store);
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

			return new Tuple<Function, bool>(fn, this_registered);
		}

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString()
		{
			return string.Format("[Call for {0} with ({1})]", Name, Arguments);
		}
    }
}
