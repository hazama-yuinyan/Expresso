using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using Expresso.BuiltIns;
using Expresso.Helpers;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 関数定義。
	/// Represents a function.
    /// </summary>
    public class Function : Statement
    {
        /// <summary>
        /// 関数名。
		/// The name of the function.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 仮引数リスト。
		/// The parameter list.
        /// </summary>
        public List<Argument> Parameters { get; internal set; }

        /// <summary>
        /// 関数本体。
		/// The body of the function.
        /// </summary>
        public Block Body { get; internal set; }
		
		/// <summary>
		/// 戻り値の型
		/// The type of return value.
		/// </summary>
		public TYPES ReturnType {get; internal set;}

        /// <summary>
        /// 関数内で定義されたローカル変数一覧。
		/// The list of local variables defined in this function.
        /// </summary>
        public IEnumerable<Parameter> LocalVariables
        {
            get
            {
				if(Body == null) return Enumerable.Empty<Parameter>();
                return this.Body.LocalVariables;
            }
        }

        /// <summary>
        /// 関数＋引数名、
        /// f(x, y) {x + y;} の f(x, y) の部分を文字列として返す。
        /// </summary>
        /// <returns>関数シグニチャ</returns>
        public string Signature()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.Name);
            sb.Append("(");

            bool first = true;

            foreach (var param in this.Parameters)
            {
                if (first) first = false;
                else sb.Append(", ");

                sb.Append(param.ToString());
            }

            sb.Append(") => ");
			sb.Append(this.ReturnType);

            return sb.ToString();
        }

        public override NodeType Type
        {
            get { return NodeType.Function; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Function;

            if (x == null) return false;

            if (this.Name != x.Name) return false;

            if (this.Parameters.Count != x.Parameters.Count) return false;

            for (int i = 0; i < this.Parameters.Count; i++)
            {
                if (!this.Parameters[i].Equals(x.Parameters[i])) return false;
            }

            return this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Parameters.GetHashCode() ^ this.Body.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			return this.Body.Run(varStore);
        }
		
		public override string ToString()
		{
			return this.Signature();
		}
    }

	public class NativeFunction : Function
	{
		public override string ToString()
		{
			var signature = this.Signature();
			return "(Native)" + signature;
		}
	}

	public class NativeFunctionNullary<ReturnType> : NativeFunction
	{
		private Func<ReturnType> func;

		public NativeFunctionNullary(string Name, List<Argument> Params, Func<ReturnType> Func)
		{
			this.Name = Name;
			this.Parameters = Params;
			this.Body = null;
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementaionHelpers.GetTypeInExpresso(t);
			this.func = Func;
		}

		internal override object Run(VariableStore varStore)
		{
			ReturnType result = func.Invoke();
			return new ExpressoPrimitive{Value = result, Type = this.ReturnType};
		}
	}

	public class NativeFunctionUnary<ReturnType, Param1> : NativeFunction
	{
		private Func<Param1, ReturnType> func;

		public NativeFunctionUnary(string Name, Argument Param, Func<Param1, ReturnType> Func)
		{
			this.Name = Name;
			this.Parameters = new List<Argument>{Param};
			this.Body = null;
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementaionHelpers.GetTypeInExpresso(t);
			this.func = Func;
		}

		internal override object Run(VariableStore varStore)
		{
			ExpressoPrimitive arg = varStore.Get(this.Parameters[0].Name) as ExpressoPrimitive;
			dynamic val = arg.Value;
			ReturnType result = func.Invoke(val);
			return new ExpressoPrimitive{Value = result, Type = this.ReturnType};
		}
	}

	public class NativeFunctionBiary<ReturnType, Param1, Param2> : NativeFunction
	{
		private Func<Param1, Param2, ReturnType> func;

		public NativeFunctionBiary(string Name, Argument Parameter1, Argument Parameter2, Func<Param1, Param2, ReturnType> Func)
		{
			this.Name = Name;
			this.Parameters = new List<Argument>{Parameter1, Parameter2};
			this.Body = null;
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementaionHelpers.GetTypeInExpresso(t);
			this.func = Func;
		}

		internal override object Run(VariableStore varStore)
		{
			ExpressoPrimitive arg1 = varStore.Get(this.Parameters[0].Name) as ExpressoPrimitive;
			ExpressoPrimitive arg2 = varStore.Get(this.Parameters[1].Name) as ExpressoPrimitive;
			dynamic val1 = arg1.Value;
			dynamic val2 = arg2.Value;
			ReturnType result = func.Invoke(val1, val2);
			return new ExpressoPrimitive{Value = result, Type = this.ReturnType};
		}
	}
}
