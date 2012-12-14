using System.Collections.Generic;
using System.Linq;
using Expresso.Interpreter;
using Expresso.Helpers;
using Expresso.Compiler;

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
		/// objects having function objects and they are resolved on runtime.
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
						throw new EvalException("Not callable: " + obj.ToString());

					fn = method.Method;
					if(method.Inst != null){
						child.Add(0, method.Inst);	//このメソッド呼び出しのthisオブジェクトを登録する
						if(Arguments.Count == 0 || Arguments[0] != null)
							Arguments.Insert(0, null);

						this_registered = true;
					}
					method_info = method;	//毎回リファレンスを辿るのは遅いと思われるので、キャッシュしておく
				}else{
					fn = method_info.Method;
					child.Add(0, method_info.Inst);	//このメソッド呼び出しのthisオブジェクトを登録する
					this_registered = true;
				}
			}else{
    	        fn = Function;
			}

			for(int i = (this_registered) ? 1 : 0; i < fn.Parameters.Count; ++i){	//実引数をローカル変数として変数テーブルに追加する
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
