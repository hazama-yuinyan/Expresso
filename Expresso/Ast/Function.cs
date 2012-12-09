using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using ExprTree = System.Linq.Expressions;
using Expresso.Builtins;
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
		/// 戻り値の型。
		/// The type of the return value.
		/// </summary>
		public TypeAnnotation ReturnType {get; internal set;}

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

		public Function(string name, List<Argument> parameters, Block body, TypeAnnotation returnType, VariableStore environ = null)
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
		public NativeFunction(string name, List<Argument> parameters, Block body, TypeAnnotation returnType) :
			base(name, parameters, body, returnType, null){}

		public override string ToString()
		{
			var signature = this.Signature();
			return "(Native)" + signature;
		}
	}

	/// <summary>
	/// Represents an N-ary native function. It can also represent a native method if the first parameter is the instance.
	/// </summary>
	public class NativeFunctionNAry : NativeFunction
	{
		private ExprTree.LambdaExpression func;
		private Delegate compiled = null;

		public NativeFunctionNAry(string name, ExprTree.LambdaExpression func) :
			base(name, null, null, new TypeAnnotation(TYPES.VAR))
		{
			this.func = func;
			var parameters = func.Parameters;
			if(parameters.Count > 0)
				this.Parameters = new List<Argument>();

			int i = 0;
			foreach(var param in parameters){
				var formal = new Argument{Ident = new Identifier(param.Name, ImplementationHelpers.GetTypeAnnoInExpresso(param.Type), i)};
				this.Parameters.Add(formal);
				++i;
			}
		}

		internal override object Run(VariableStore varStore)
		{
			if(compiled == null)
				compiled = func.Compile();

			var args = new object[Parameters.Count];
			for(int i = 0; i < Parameters.Count; ++i)
				args[i] = varStore.Get(i);

			return compiled.DynamicInvoke(args);
		}
	}

	public class NativeLambdaNullary : NativeFunction
	{
		private Func<object> func;

		public NativeLambdaNullary(string name, Func<object> func, TypeAnnotation returnType = null) :
			base(name, null, null, (returnType != null) ? returnType : new TypeAnnotation(TYPES.VAR))
		{
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			return func();
		}
	}

	public class NativeLambdaUnary : NativeFunction
	{
		private Func<object, object> func;

		public NativeLambdaUnary(string name, Argument param, Func<object, object> func, TypeAnnotation returnType = null) :
			base(name, new List<Argument>{param}, null, (returnType != null) ? returnType : new TypeAnnotation(TYPES.VAR))
		{
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			object arg = varStore.Get(0);
			return func(arg);
		}
	}

	public class NativeLambdaBinary : NativeFunction
	{
		private Func<object, object, object> func;

		public NativeLambdaBinary(string name, Argument param1, Argument param2, Func<object, object, object> func, TypeAnnotation returnType = null) :
			base(name, new List<Argument>{param1, param2}, null, (returnType != null) ? returnType : new TypeAnnotation(TYPES.VAR))
		{
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			object arg1 = varStore.Get(0);
			object arg2 = varStore.Get(1);
			return func(arg1, arg2);
		}
	}

	public class NativeLambdaTernary : NativeFunction
	{
		private Func<object, object, object, object> func;

		public NativeLambdaTernary(string name, Argument param1, Argument param2, Argument param3,
		                           Func<object, object, object, object> func, TypeAnnotation returnType = null) :
			base(name, new List<Argument>{param1, param2, param3}, null, (returnType != null) ? returnType : new TypeAnnotation(TYPES.VAR))
		{
			this.func = func;
		}

		internal override object Run(VariableStore varStore)
		{
			object arg1 = varStore.Get(0);
			object arg2 = varStore.Get(1);
			object arg3 = varStore.Get(2);
			return func(arg1, arg2, arg3);
		}
	}
}
