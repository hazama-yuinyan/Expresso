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
		/// It can be null if the function takes no parameters.
        /// </summary>
        public List<Argument> Parameters { get; internal set; }

        /// <summary>
        /// 関数本体。
		/// The body of the function.
		/// It can be null if the function is native.
        /// </summary>
        public Block Body { get; internal set; }
		
		/// <summary>
		/// 戻り値の型
		/// The type of the return value.
		/// </summary>
		public TYPES ReturnType {get; internal set;}

		/// <summary>
		/// このクロージャが定義された時の環境。
		/// The environment in which the function is defined. It can be null if the function isn't a closure.
		/// </summary>
		public VariableStore Environment{get; internal set;}

        /// <summary>
        /// 関数内で定義されたローカル変数一覧。
		/// The list of local variables defined in this function.
        /// </summary>
        public IEnumerable<Identifier> LocalVariables
        {
            get
            {
				if(Body == null) return Enumerable.Empty<Identifier>();
                return this.Body.LocalVariables;
            }
        }

		public Function(string name, List<Argument> parameters, Block body, TYPES returnType, VariableStore environ = null)
		{
			Name = name;
			Parameters = parameters;
			Body = body;
			ReturnType = returnType;
			Environment = environ;
		}

        /// <summary>
        /// 関数＋引数名、
        /// f(x, y) => some_type {x + y;} の f(x, y) => some_type の部分を文字列として返す。
		/// Returns the function signature.
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
		public NativeFunction(string name, List<Argument> parameters, Block body, TYPES returnType, VariableStore environ) :
			base(name, parameters, body, returnType, environ){}

		public override string ToString()
		{
			var signature = this.Signature();
			return "(Native)" + signature;
		}
	}

	public class NativeFunctionNullary<ReturnType> : NativeFunction
	{
		private Func<ReturnType> func;

		public NativeFunctionNullary(string name, Func<ReturnType> func) : base(name, null, null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			if(ReturnType == TYPES.UNDEF){
				func.Invoke();
				return null;
			}
			return ImplementationHelpers.WrapObject(func.Invoke(), ReturnType);
		}
	}

	public class NativeFunctionUnaryRef<ReturnType, Param1> : NativeFunction
		where Param1 : class
	{
		private Func<Param1, ReturnType> func;

		public NativeFunctionUnaryRef(string name, Identifier param, Func<Param1, ReturnType> func) :
			base(name, ImplementationHelpers.CreateArgList(param), null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			Param1 arg = varStore.Get(0) as Param1;
			if(arg == null)
				throw new EvalException("Can not cast the 1st argument.");

			if(ReturnType == TYPES.UNDEF){
				func.Invoke(arg);
				return null;
			}
			return ImplementationHelpers.WrapObject(func.Invoke(arg), ReturnType);
		}
	}

	public class NativeFunctionBinaryRef<ReturnType, Param1, Param2> : NativeFunction
		where Param1 : class where Param2 : class
	{
		private Func<Param1, Param2, ReturnType> func;

		public NativeFunctionBinaryRef(string name, Identifier param1, Identifier param2, Func<Param1, Param2, ReturnType> func) :
			base(name, ImplementationHelpers.CreateArgList(param1, param2), null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			Param1 arg1 = varStore.Get(0) as Param1;
			if(arg1 == null)
				throw new EvalException("Can not cast the 1st argument.");

			Param2 arg2 = varStore.Get(1) as Param2;
			if(arg2 == null)
				throw new EvalException("Can not cast the 2nd argument.");

			if(ReturnType == TYPES.UNDEF){
				func.Invoke(arg1, arg2);
				return null;
			}
			return ImplementationHelpers.WrapObject(func.Invoke(arg1, arg2), ReturnType);
		}
	}

	public class NativeFunctionTernaryRef<ReturnType, Param1, Param2, Param3> : NativeFunction
		where Param1 : class where Param2 : class where Param3 : class
	{
		private Func<Param1, Param2, Param3, ReturnType> func;

		public NativeFunctionTernaryRef(string name, Identifier param1, Identifier param2, Identifier param3, Func<Param1, Param2, Param3, ReturnType> func) :
			base(name, ImplementationHelpers.CreateArgList(param1, param2, param3), null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			Param1 arg1 = varStore.Get(0) as Param1;
			if(arg1 == null)
				throw new EvalException("Can not cast the 1st argument.");

			Param2 arg2 = varStore.Get(1) as Param2;
			if(arg2 == null)
				throw new EvalException("Can not cast the 2nd argument.");

			Param3 arg3 = varStore.Get(2) as Param3;
			if(arg3 == null)
				throw new EvalException("Can not cast the 3rd argument.");

			if(ReturnType == TYPES.UNDEF){
				func.Invoke(arg1, arg2, arg3);
				return null;
			}
			return ImplementationHelpers.WrapObject(func.Invoke(arg1, arg2, arg3), ReturnType);
		}
	}

	public class NativeFunctionUnaryVal<ReturnType, Param1> : NativeFunction
		where Param1 : struct
	{
		private Func<Param1, ReturnType> func;

		public NativeFunctionUnaryVal(string name, Identifier param, Func<Param1, ReturnType> func) :
			base(name, ImplementationHelpers.CreateArgList(param), null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			dynamic arg = varStore.Get(0);

			if(ReturnType == TYPES.UNDEF){
				func.Invoke(arg);
				return null;
			}
			return func.Invoke(arg);
		}
	}

	public class NativeFunctionBinaryVal<ReturnType, Param1, Param2> : NativeFunction
		where Param1 : struct where Param2 : struct
	{
		private Func<Param1, Param2, ReturnType> func;

		public NativeFunctionBinaryVal(string name, Identifier parameter1, Identifier parameter2, Func<Param1, Param2, ReturnType> func) :
			base(name, ImplementationHelpers.CreateArgList(parameter1, parameter2), null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			dynamic arg1 = varStore.Get(0);
			dynamic arg2 = varStore.Get(1);

			if(ReturnType == TYPES.UNDEF){
				func.Invoke(arg1, arg2);
				return null;
			}
			return func.Invoke(arg1, arg2);
		}
	}

	public class NativeFunctionTernaryVal<ReturnType, Param1, Param2, Param3> : NativeFunction
		where Param1 : struct where Param2 : struct where Param3 : struct
	{
		private Func<Param1, Param2, Param3, ReturnType> func;

		public NativeFunctionTernaryVal(string name, Identifier param1, Identifier param2, Identifier param3, Func<Param1, Param2, Param3, ReturnType> func) :
			base(name, ImplementationHelpers.CreateArgList(param1, param2, param3), null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			dynamic arg1 = varStore.Get(0);
			dynamic arg2 = varStore.Get(1);
			dynamic arg3 = varStore.Get(2);

			if(ReturnType == TYPES.UNDEF){
				func.Invoke(arg1, arg2, arg3);
				return null;
			}
			return func.Invoke(arg1, arg2, arg3);
		}
	}

	public class NativeMethodNullary<ReturnType, ThisType> : NativeFunction
		where ThisType : class
	{
		private Func<ThisType, ReturnType> method;

		public NativeMethodNullary(string name, Func<ThisType, ReturnType> method) :
			base(name, null, null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.method = method;
			this.Parameters =
				ImplementationHelpers.CreateArgList(new Identifier("this", ImplementationHelpers.GetTypeInExpresso(typeof(ThisType)), 0));
		}

		internal override object Run(VariableStore varStore)
		{
			var exs_obj = varStore.Get(0) as ExpressoClass.ExpressoObj;
			var _this = exs_obj.GetMember(0) as ThisType;
			if(_this == null)
				throw new EvalException("Can not call the method " + method + " on that type of object.");

			if(ReturnType == TYPES.UNDEF){
				method.Invoke(_this);
				return null;
			}
			return ImplementationHelpers.WrapObject(method.Invoke(_this), ReturnType);
		}
	}

	public class NativeMethodUnary<ReturnType, ThisType, Param1> : NativeFunction
		where ThisType : class
	{
		private Func<ThisType, Param1, ReturnType> method;

		public NativeMethodUnary(string name, Identifier param, Func<ThisType, Param1, ReturnType> method) :
			base(name, null, null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.method = method;
			this.Parameters =
				ImplementationHelpers.CreateArgList(new Identifier("this", ImplementationHelpers.GetTypeInExpresso(typeof(ThisType)), 0),
				                                    param);
		}

		internal override object Run(VariableStore varStore)
		{
			var exs_obj = varStore.Get(0) as ExpressoClass.ExpressoObj;
			var _this = exs_obj.GetMember(0) as ThisType;
			if(_this == null)
				throw new EvalException("Can not call the method " + method + " on that type of object.");

			dynamic param1 = varStore.Get(1);

			if(ReturnType == TYPES.UNDEF){
				method.Invoke(_this, param1);
				return null;
			}
			return ImplementationHelpers.WrapObject(method.Invoke(_this, param1), ReturnType);
		}
	}

	public class NativeMethodBinary<ReturnType, ThisType, Param1, Param2> : NativeFunction
		where ThisType : class
	{
		private Func<ThisType, Param1, Param2, ReturnType> method;

		public NativeMethodBinary(string name, Identifier param1, Identifier param2, Func<ThisType, Param1, Param2, ReturnType> method) :
			base(name, null, null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.method = method;
			this.Parameters =
				ImplementationHelpers.CreateArgList(new Identifier("this", ImplementationHelpers.GetTypeInExpresso(typeof(ThisType)), 0),
				                                    param1, param2);
		}

		internal override object Run(VariableStore varStore)
		{
			var exs_obj = varStore.Get(0) as ExpressoClass.ExpressoObj;
			var _this = exs_obj.GetMember(0) as ThisType;
			if(_this == null)
				throw new EvalException("Can not call the method " + method + " on that type of object.");

			dynamic param1 = varStore.Get(1);
			dynamic param2 = varStore.Get(2);

			if(ReturnType == TYPES.UNDEF){
				method.Invoke(_this, param1, param2);
				return null;
			}
			return ImplementationHelpers.WrapObject(method.Invoke(_this, param1, param2), ReturnType);
		}
	}

	public class NativeMethodTernary<ReturnType, ThisType, Param1, Param2, Param3> : NativeFunction
		where ThisType : class
	{
		private Func<ThisType, Param1, Param2, Param3, ReturnType> method;

		public NativeMethodTernary(string name, Identifier param1, Identifier param2, Identifier param3, Func<ThisType, Param1, Param2, Param3, ReturnType> method) :
			base(name, null, null, TYPES.UNDEF, null)
		{
			Type t = typeof(ReturnType);
			this.ReturnType = ImplementationHelpers.GetTypeInExpresso(t);
			this.method = method;
			this.Parameters =
				ImplementationHelpers.CreateArgList(new Identifier("this", ImplementationHelpers.GetTypeInExpresso(typeof(ThisType)), 0),
				                                    param1, param2, param3);
		}

		internal override object Run(VariableStore varStore)
		{
			var exs_obj = varStore.Get(0) as ExpressoClass.ExpressoObj;
			var _this = exs_obj.GetMember(0) as ThisType;
			if(_this == null)
				throw new EvalException("Can not call the method " + method + " on that type of object.");

			dynamic param1 = varStore.Get(1);
			dynamic param2 = varStore.Get(2);
			dynamic param3 = varStore.Get(3);

			if(ReturnType == TYPES.UNDEF){
				method.Invoke(_this, param1, param2, param3);
				return null;
			}
			return ImplementationHelpers.WrapObject(method.Invoke(_this, param1, param2, param3), ReturnType);
		}
	}
}
