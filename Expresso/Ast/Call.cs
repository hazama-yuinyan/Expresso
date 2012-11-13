using System.Collections.Generic;
using System.Linq;
using Expresso.Interpreter;
using Expresso.Helpers;

namespace Expresso.Ast
{
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
			Function fn;
			if(Reference != null){
				var callable = Reference.Run(varStore) as Function;
				if(callable == null)
					throw new EvalException("Not callable: " + callable.ToString());

				fn = callable;
			}else{
    	        fn = Function;
			}
			var child = new VariableStore{Parent = varStore};
			for (int i = 0; i < fn.Parameters.Count; ++i) {	//実引数をローカル変数として変数テーブルに追加する
				child.Add(fn.Parameters[i].Name, (i < Arguments.Count) ? Arguments[i].Run(varStore) : fn.Parameters[i].Option.Run(varStore));
			}
			var local_vars = fn.LocalVariables;
			if(local_vars.Any()){					//Checking for its emptiness
				foreach(var local in local_vars){	//関数内で定義されているローカル変数を予め変数テーブルに追加しておく
					child.Add(local.Name, ImplementaionHelpers.GetDefaultValueFor(local.ParamType));
				}
			}

			return fn.Run(child);
        }

		public override string ToString ()
		{
			return string.Format("[Call for {0} with ({1})]", Name, Arguments);
		}
    }
}
