using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// モジュール宣言式。
	/// The module declaration.
	/// </summary>
	public class ModuleDeclaration : Statement
	{
		/// <summary>
		/// モジュール名。Expressoには明示的なモジュール宣言がないので、たいていはファイル名になる。
		/// The name of the module. Since Expresso doesn't have an explicit module declaration, it would be very likely to be
		/// the file name.
		/// If the module contains the main function, it would be called the "main" module.
		/// </summary>
		public string Name{get; internal set;}

		public List<Statement> Requires{get; internal set;}

		/// <summary>
		/// モジュールメンバの定義式郡。
		/// The declarations for members.
		/// </summary>
		public List<Statement> Declarations { get; internal set; }

		/// <summary>
		/// Declarationsのn番目の定義をエクスポートするかどうかをあらわす。
		/// A map indicating what declaration would be exported.
		/// </summary>
		public List<bool> ExportMap{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.ModuleDecl; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ModuleDeclaration;

            if (x == null) return false;

            return this.Name == x.Name && this.Declarations.Equals(x.Declarations);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Declarations.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
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
						throw new EvalException("A module declaration can not have that type of statements!");
					}
				}else if(decl.Item1 is Function){
					var method = (Function)decl.Item1;
					decl_target.Add(method.Name, offset++);
					members.Add(method);
				}else if(decl.Item1 is TypeDeclaration){
					var type_decl = (TypeDeclaration)decl.Item1;
					var type_def = type_decl.Run(varStore);
					decl_target.Add(type_decl.Name, offset++);
					members.Add(type_def);
				}else{
					throw new EvalException("A module declaration can not have that type of statements!");
				}
			}

			var module_def = new ModuleDefinition(Name, internals, exported, members.ToArray());
			var module_inst = new ExpressoObj(module_def);
			ExpressoModule.AddModule(module_def.Name, module_inst);
			return null;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString()
		{
			return string.Format("Declaration of module {0}", Name);
		}
	}
}

