using System;
using System.Collections.Generic;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using System.Text;

namespace Expresso.Ast
{
    /// <summary>
    /// 関数定義。
    /// Represents a function declaration.
    /// [ Modifiers ] "def" Name '(' [ Arguments ] ')' [ "->" ReturnType ] '{' Body '}' ;
    /// </summary>
    public class FunctionDeclaration : EntityDeclaration
    {
        public override SymbolKind SymbolKind{
            get{
                return SymbolKind.Method;
            }
        }

        /// <summary>
        /// 仮引数リスト。
		/// The formal parameter list.
        /// It can be empty if the function takes no parameters.
        /// </summary>
        public AstNodeCollection<ParameterDeclaration> Parameters{
            get{return GetChildrenByRole(Roles.Parameter);}
		}

        /// <summary>
        /// 関数本体。
		/// The body of the function.
		/// It can be null if the function is native.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }
		
		/// <summary>
		/// このクロージャが定義された時の環境。
		/// The environment in which the function is defined. It can be null if the function isn't a closure.
		/// </summary>
        //public Stack<object> Environment{get; internal set;}

        public TextLocation Header{get; internal set;}

		internal bool HasReturn{get; set;}

		/// <summary>
		/// Indicates whether the function is static.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is static; otherwise, <c>false</c>.
		/// </value>
		public bool IsStatic{
            get{
                return HasModifier(Modifiers.Static);
            }
        }

        public FunctionDeclaration(string funcName, IEnumerable<ParameterDeclaration> formalParameters,
            BlockStatement body, AstType returnType, Modifiers modifiers, TextLocation loc)
            : base(loc, body.EndLocation)
		{
            SetChildByRole(Roles.Identifier, AstNode.MakeIdentifier(funcName));
            if(formalParameters != null){
                foreach(var param in formalParameters)
                    AddChild(param, Roles.Parameter);
            }
            Body = body;
            AddChild(returnType, Roles.Type);
            SetModifiers(this, modifiers);
		}

        /// <summary>
        /// 関数＋引数名、
        /// f(x, y) -> some_type {x + y;} の f(x, y) -> some_type の部分を文字列として返す。
		/// Returns the function signature.
        /// </summary>
        /// <returns>関数シグニチャ</returns>
        public string Signature()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.Name);
            sb.Append("(");

            bool first = true;

            foreach (var param in this.Parameters){
                if(first)
                    first = false;
                else
                    sb.Append(", ");

                sb.Append(param.ToString());
            }

            sb.Append(") => ");
			sb.Append(this.ReturnType);

            return sb.ToString();
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitFunctionDeclaration(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitFunctionDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitFunctionDeclaration(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as FunctionDeclaration;
            return o != null && Name == o.Name && Parameters.DoMatch(o.Parameters, match)
                && ReturnType.DoMatch(o.ReturnType, match) && Body.DoMatch(o.Body, match);
        }
    }

	/*public class NativeFunction : FunctionDefinition
	{
		public NativeFunction(string name, List<Argument> parameters, Block body, TypeAnnotation returnType, bool isStatic) :
			base(name, parameters, body, returnType, isStatic, null){}

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
		ExprTree.LambdaExpression func;
		Delegate compiled = null;

		public NativeFunctionNAry(string name, ExprTree.LambdaExpression func) :
			base(name, null, null, new TypeAnnotation(ObjectTypes.VAR), false)
		{
			this.func = func;
			var parameters = func.Parameters;
			if(parameters.Count > 0)
				this.Parameters = new List<Argument>();

			int i = 0;
			foreach(var param in parameters){
				var formal = new Argument{Ident = new Identifier(param.Name, ImplementationHelpers.GetTypeAnnoInExpresso(param.Type), null, i)};
				this.Parameters.Add(formal);
				++i;
			}
		}

		/*internal override object Run(VariableStore varStore)
		{
			if(compiled == null)
				compiled = func.Compile();

			var args = new object[Parameters.Count];
			for(int i = 0; i < Parameters.Count; ++i)
				args[i] = varStore.Get(i);

			return compiled.DynamicInvoke(args);
		}*/
	//}

	/// <summary>
	/// Represents a nullary native lambda function. When you refer to a static method, use this class instead of the
	/// <see cref="NativeFunctionNAry"/>
	/// </summary>
	/*public class NativeLambdaNullary : NativeFunction
	{
		Func<object> func;

		public NativeLambdaNullary(string name, Func<object> func, TypeAnnotation returnType = null) :
			base(name, null, null, (returnType != null) ? returnType : new TypeAnnotation(ObjectTypes.VAR), true)
		{
			this.func = func;
		}

		/*internal override object Run(VariableStore varStore)
		{
			return func();
		}*/
	//}

	/// <summary>
	/// Represents a native unary lambda function. When you refer to a static method, use this class instead of the
	/// <see cref="NativeFunctionNAry"/>
	/// </summary>
	/*public class NativeLambdaUnary : NativeFunction
	{
		Func<object, object> func;

		public NativeLambdaUnary(string name, Argument param, Func<object, object> func, TypeAnnotation returnType = null) :
			base(name, new List<Argument>{param}, null, (returnType != null) ? returnType : new TypeAnnotation(ObjectTypes.VAR), true)
		{
			this.func = func;
		}

		/*internal override object Run(VariableStore varStore)
		{
			object arg = varStore.Get(0);
			return func(arg);
		}*/
	//}

	/// <summary>
	/// Represents a native binary lambda function. When you refer to a static method, use this class instead of the
	/// <see cref="NativeFunctionNAry"/>
	/// </summary>
	/*public class NativeLambdaBinary : NativeFunction
	{
		Func<object, object, object> func;

		public NativeLambdaBinary(string name, Argument param1, Argument param2, Func<object, object, object> func, TypeAnnotation returnType = null) :
			base(name, new List<Argument>{param1, param2}, null, (returnType != null) ? returnType : new TypeAnnotation(ObjectTypes.VAR), true)
		{
			this.func = func;
		}

		/*internal override object Run(VariableStore varStore)
		{
			object arg1 = varStore.Get(0);
			object arg2 = varStore.Get(1);
			return func(arg1, arg2);
		}*/
	//}

	/// <summary>
	/// Represents a native ternary lambda function. When you refer to a static method, use this class instead of the
	/// <see cref="NativeFunctionNAry"/>
	/// </summary>
	/*public class NativeLambdaTernary : NativeFunction
	{
		Func<object, object, object, object> func;

		public NativeLambdaTernary(string name, Argument param1, Argument param2, Argument param3,
		                           Func<object, object, object, object> func, TypeAnnotation returnType = null) :
			base(name, new List<Argument>{param1, param2, param3}, null,
			(returnType != null) ? returnType : new TypeAnnotation(ObjectTypes.VAR), true)
		{
			this.func = func;
		}

		/*internal override object Run(VariableStore varStore)
		{
			object arg1 = varStore.Get(0);
			object arg2 = varStore.Get(1);
			object arg3 = varStore.Get(2);
			return func(arg1, arg2, arg3);
		}*/
	//}
}
