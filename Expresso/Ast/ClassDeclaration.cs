using System;
using System.Collections.Generic;
using Expresso.Builtins;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// クラス宣言式。
	/// The class declaration.
	/// </summary>
	public class ClassDeclaration : Statement
	{
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
            get { return NodeType.ClassDecl; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ClassDeclaration;

            if (x == null) return false;

            return this.Name == x.Name && this.Declarations.Equals(x.Declarations);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Declarations.GetHashCode();
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
						case TYPES._LABEL_PUBLIC:
							decl_target = publics;
							break;

						case TYPES._LABEL_PRIVATE:
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

			var class_def = new ExpressoClass.ClassDefinition(Name, privates, publics);
			class_def.Members = members.ToArray();
			ExpressoClass.AddClass(class_def);
			return null;
        }

		public override string ToString()
		{
			return string.Format("class {0}", Name);
		}
	}
}

