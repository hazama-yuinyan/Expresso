using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using ExprTree = System.Linq.Expressions;

using Expresso.Builtins;
using Expresso.Runtime;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Compiler.Meta;
using Expresso.Utils;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// 関数定義。
	/// Represents a function definition.
    /// </summary>
    public class FunctionDefinition : ScopeStatement
    {
		readonly string name;
		readonly Argument[] parameters;

        /// <summary>
        /// 関数名。
		/// The name of the function. It can be null if this definition is for a lambda.
        /// </summary>
        public string Name {
			get{return name;}
		}

        /// <summary>
        /// 仮引数リスト。
		/// The formal parameter list.
		/// It can be null if the function takes no parameters.
        /// </summary>
        public Argument[] Parameters {
			get{return parameters;}
		}

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
		public Stack<object> Environment{get; internal set;}

		public SourceLocation Header{get; internal set;}

		internal bool HasReturn{get; set;}

		/// <summary>
		/// Indicates whether the function is static.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is static; otherwise, <c>false</c>.
		/// </value>
		public bool IsStatic{get; internal set;}

        /// <summary>
        /// 関数内で定義されたローカル変数一覧。
		/// The list of local variables defined in this function.
        /// </summary>
        public IEnumerable<Identifier> LocalVariables
        {
            get{
				if(Body == null) return Enumerable.Empty<Identifier>();
                return this.Body.LocalVariables;
            }
        }

		public FunctionDefinition(string funcName, Argument[] formalParameters, Block body, TypeAnnotation returnType, bool isStatic = false,
		                Stack<object> environ = null)
		{
			name = funcName;
			parameters = formalParameters;
			Body = body;
			ReturnType = returnType;
			IsStatic = isStatic;
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

            foreach (var param in this.Parameters){
                if (first) first = false;
                else sb.Append(", ");

                sb.Append(param.ToString());
            }

            sb.Append(") => ");
			sb.Append(this.ReturnType);

            return sb.ToString();
        }

        public override NodeType Type{
            get{return NodeType.FunctionDef;}
        }

		internal override bool ExposesLocalVariable(ExpressoVariable variable)
		{
			return false;
		}

		internal override bool TryBindOuter(ScopeStatement from, ExpressoReference reference, out ExpressoVariable variable)
		{
			// Functions expose their locals to direct access
			//ContainsNestedFreeVariables = true;
			if(TryGetVariable(reference.Name, out variable)){
				//variable.AccessedInNestedScope = true;
				
				/*if(variable.Kind == VariableKind.Local || variable.Kind == VariableKind.Parameter){
					from.AddFreeVariable(variable, true);
					
					for(ScopeStatement scope = from.Parent; scope != this; scope = scope.Parent){
						scope.AddFreeVariable(variable, false);
					}
					
					AddCellVariable(variable);
				} else {
					from.AddReferencedGlobal(reference.Name);
				}*/
				return true;
			}
			return false;
		}
		
		internal override ExpressoVariable BindReference(ExpressoNameBinder binder, ExpressoReference reference)
		{
			ExpressoVariable variable;
			
			// First try variables local to this scope
			if(TryGetVariable(reference.Name, out variable)){
				if(variable.Kind == VariableKind.Global){
					AddReferencedGlobal(reference.Name);
				}
				return variable;
			}
			
			// Try to bind in outer scopes
			for(ScopeStatement parent = Parent; parent != null; parent = parent.Parent){
				if(parent.TryBindOuter(this, reference, out variable)){
					return variable;
				}
			}
			
			return null;
		}
		
		
		internal override void Bind(ExpressoNameBinder binder)
		{
			base.Bind(binder);
			Verify(binder);
			
			/*if(((PythonContext)binder.Context.SourceUnit.LanguageContext).PythonOptions.FullFrames){
				// force a dictionary if we have enabled full frames for sys._getframe support
				NeedsLocalsDictionary = true;
			}*/
		}
		
		internal override void FinishBind(ExpressoNameBinder binder)
		{
			/*foreach(var param in parameters) {
				_variableMapping[param.PythonVariable] = param.FinishBind(NeedsLocalsDictionary);
			}*/
			base.FinishBind(binder);
		}
		
		void Verify(ExpressoNameBinder binder) {
			/*if (ContainsImportStar && IsClosure) {
				binder.ReportSyntaxError(
					String.Format(
					System.Globalization.CultureInfo.InvariantCulture,
					"import * is not allowed in function '{0}' because it is a nested function",
					Name),
					this);
			}
			if (ContainsImportStar && Parent is FunctionDefinition) {
				binder.ReportSyntaxError(
					String.Format(
					System.Globalization.CultureInfo.InvariantCulture,
					"import * is not allowed in function '{0}' because it is a nested function",
					Name),
					this);
			}
			if (ContainsImportStar && ContainsNestedFreeVariables) {
				binder.ReportSyntaxError(
					String.Format(
					System.Globalization.CultureInfo.InvariantCulture,
					"import * is not allowed in function '{0}' because it contains a nested function with free variables",
					Name),
					this);
			}
			if (ContainsUnqualifiedExec && ContainsNestedFreeVariables) {
				binder.ReportSyntaxError(
					String.Format(
					System.Globalization.CultureInfo.InvariantCulture,
					"unqualified exec is not allowed in function '{0}' because it contains a nested function with free variables",
					Name),
					this);
			}
			if (ContainsUnqualifiedExec && IsClosure) {
				binder.ReportSyntaxError(
					String.Format(
					System.Globalization.CultureInfo.InvariantCulture,
					"unqualified exec is not allowed in function '{0}' because it is a nested function",
					Name),
					this);
			}*/
		}

        public override bool Equals(object obj)
        {
            var x = obj as FunctionDefinition;

            if (x == null) return false;

            if (this.Name != x.Name) return false;

            if (this.Parameters.Length != x.Parameters.Length) return false;

            for (int i = 0; i < this.Parameters.Length; i++)
            {
                if (!this.Parameters[i].Equals(x.Parameters[i])) return false;
            }

            return this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Parameters.GetHashCode() ^ this.Body.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			return this.Body.Run(varStore);
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				foreach(var param in parameters)
					param.Walk(walker);

				Body.Walk(walker);
			}
			walker.PostWalk(this);
		}
		
		public override string ToString()
		{
			return IsStatic ? "<static> " + this.Signature() : this.Signature();
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
