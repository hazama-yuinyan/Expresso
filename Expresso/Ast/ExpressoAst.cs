using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// ExpressoのASTのトップレベルノード。ファイルから生成された場合はモジュールを表すが、evalによって作られた場合もある。
	/// Top-level ast for all Expresso code. Typically represents a module but could also
	/// be exec or eval code.
	/// </summary>
	public class ExpressoAst : ScopeStatement
	{
		private Statement[] body;
		private readonly string name;
		private readonly bool is_module;

		/// <summary>
		/// モジュール名。Expressoには明示的なモジュール宣言がないので、たいていはファイル名になる。
		/// The name of the module. Since Expresso doesn't have an explicit module declaration, it would be very likely to be
		/// the file name.
		/// If the module contains the main function, it would be called the "main" module.
		/// </summary>
		public string Name{
			get{return "<module>";}
		}

		/// <summary>
		/// Indicates whether the node represents a module.
		/// </summary>
		public bool IsModule{
			get{return is_module;}
		}

		/// <summary>
		/// 本体。このノードがモジュールだった場合にはrequire文とモジュールの定義文が含まれる。
		/// The body of this node. If this node represents a module, then the body includes the require statement, if any,
		/// and the definitions of the module.
		/// </summary>
		public Statement[] Body{
			get{return body;}
		}

		/// <summary>
		/// Bodyのn番目の定義をエクスポートするかどうかをあらわす。
		/// A map indicating which declarations would be exported.
		/// </summary>
		public BitArray ExportMap{get; internal set;}

		public string ModuleName{
			get{
				return is_module ? name : "<not a module>";
			}
		}

        public override NodeType Type
        {
            get { return NodeType.Toplevel; }
        }

		public ExpressoAst(Statement[] bodyStmts, string maybeModuleName = null, bool[] exportMap = null)
		{
			body = bodyStmts;
			name = maybeModuleName;
			is_module = maybeModuleName != null;
			ExportMap = new BitArray(exportMap);
		}

		/// <summary>
		/// Binds an AST and makes it capable of being reduced and compiled.  Before calling Bind an AST cannot successfully
		/// be reduced.
		/// </summary>
		public void Bind()
		{
			ExpressoNameBinder.BindAst(this, null);
		}

		#region Name binding support
		internal override bool ExposesLocalVariable(ExpressoVariable variable)
		{
			return true;
		}

		internal override ExpressoVariable BindReference(ExpressoNameBinder binder, ExpressoReference reference)
		{
			return EnsureVariable(reference.Name, reference.VariableType);
		}
		#endregion

        public override bool Equals(object obj)
        {
            var x = obj as ExpressoAst;

            if (x == null) return false;

            return this.name == x.name && this.body.Equals(x.body);
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode() ^ this.body.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			if(Requires != null){
				foreach(var require in Requires)
					require.Run(varStore);
			}

			var internals = new Dictionary<string, int>();
			var exported = new Dictionary<string, int>();
			Dictionary<string, int> decl_target = null;
			var members = new List<object>();
			int offset = 0;

			foreach(var decl in Declarations.Zip(ExportMap, (first, second) => new Tuple<Statement, bool>(first, second))){
				decl_target = (decl.Item2) ? exported : internals;

				if(decl.Item1 is ExprStatement){
					var expr_stmt = (ExprStatement)decl.Item1;
					if(expr_stmt.Expressions[0] is VarDeclaration){
						var var_decls = (VarDeclaration)expr_stmt.Expressions[0];

						object obj;
						for(int i = 0; i < var_decls.Variables.Count; ++i){
							obj = var_decls.Expressions[i].Run(varStore);
							decl_target.Add(var_decls.Variables[i].Name, offset++);
							members.Add(obj);
							//varStore.Add(var_decls.Variables[i].Offset, obj);	//モジュールスコープの変数ストアにも実体を追加しておく
						}
					}else{
						throw ExpressoOps.InvalidTypeError("A module declaration can not have that type of statements!");
					}
				}else if(decl.Item1 is FunctionDeclaration){
					var method = (FunctionDeclaration)decl.Item1;
					decl_target.Add(method.Name, offset++);
					members.Add(method);
				}else if(decl.Item1 is TypeDeclaration){
					var type_decl = (TypeDeclaration)decl.Item1;
					var type_def = type_decl.Run(varStore);
					decl_target.Add(type_decl.Name, offset++);
					members.Add(type_def);
				}else{
					throw ExpressoOps.InvalidTypeError("A module declaration can not have that type of statements!");
				}
			}

			var module_def = new ModuleDefinition(Name, internals, exported, members.ToArray());
			var module_inst = new ExpressoObj(module_def);
			ExpressoModule.AddModule(module_def.Name, module_inst);
			return null;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				if(body != null){
					foreach(var stmt in body)
						stmt.Walk(walker);
				}
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return string.Format("Declaration of module {0}", name == null ? "<anonymous>" : name);
		}
	}
}

