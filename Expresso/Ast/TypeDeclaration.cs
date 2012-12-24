using System;
using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	public enum DeclType{
		Class,
		Interface,
		Struct
	};

	/// <summary>
	/// 型宣言式。
	/// The type declaration. That is, it can represents either a class declaration, an interface declaration 
	/// or a struct declaration. 
	/// </summary>
	public class TypeDeclaration : Statement
	{
		public DeclType TargetType{get; internal set;}

		/// <summary>
		/// クラス名。
		/// The name of the class.
		/// </summary>
		public string Name{get; internal set;}

		/// <summary>
		/// 継承元のクラス名。
		/// The names of the base classes.
		/// </summary>
		public List<string> Bases{get; internal set;}

		/// <summary>
		/// クラスメンバの定義式郡。
		/// The declarations for members.
		/// </summary>
		public List<Statement> Declarations { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.TypeDecl; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as TypeDeclaration;

            if (x == null) return false;

            return this.TargetType == x.TargetType && this.Name == x.Name && this.Declarations.Equals(x.Declarations);
        }

        public override int GetHashCode()
        {
            return this.TargetType.GetHashCode() ^ this.Name.GetHashCode() ^ this.Declarations.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			var privates = new Dictionary<string, int>();
			var publics = new Dictionary<string, int>();
			Dictionary<string, int> decl_target = privates;
			var members = new List<object>();
			int offset = 0;

			foreach(var decl in Declarations){
				if(decl is ExprStatement){
					var expr_stmt = (ExprStatement)decl;
					if(expr_stmt.Expressions[0] is Constant){
						var label = (Constant)expr_stmt.Expressions[0];
						switch(label.ValType){
						case ObjectTypes._LABEL_PUBLIC:
							decl_target = publics;
							break;

						case ObjectTypes._LABEL_PRIVATE:
							decl_target = privates;
							break;

						default:
							throw new EvalException("Unknown label!");
						}
					}else if(expr_stmt.Expressions[0] is VarDeclaration){
						var var_decls = (VarDeclaration)expr_stmt.Expressions[0];

						object obj;
						for(int i = 0; i < var_decls.Variables.Count; ++i){
							obj = var_decls.Expressions[i].Run(varStore);
							decl_target.Add(var_decls.Variables[i].Name, offset++);
							members.Add(obj);
						}
					}else{
						throw new EvalException("A class declaration can not have that type of statements!");
					}
				}else if(decl is Function){
					var method = (Function)decl;
					decl_target.Add(method.Name, offset++);
					members.Add(method);
				}else{
					throw new EvalException("A class declaration can not have that type of statements!");
				}
			}

			object type_def = null;
			switch(TargetType){
			case DeclType.Class:
				type_def = new ClassDefinition(Name, privates, publics, members.ToArray(), null);
				break;
			}
			return type_def;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString()
		{
			return string.Format("Declaration of {0} {1}", TargetType, Name);
		}
	}
}

