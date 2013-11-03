using System;
using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	public enum DeclType{
		Class,
		Interface,
		Struct
	};

	/// <summary>
	/// 型定義文。
	/// The type definition. That is, it can represents either a class definition, an interface definition 
	/// or a struct definition. 
	/// </summary>
	public class TypeDefinition : ScopeStatement
	{
		string name;
		string[] base_names;
		Statement[] body;
		DeclType target_type;

		public DeclType TargetType{
			get{return target_type;}
		}

		/// <summary>
		/// 型名。
		/// The name of the type.
		/// </summary>
		public string Name{
			get{return name;}
		}

		/// <summary>
		/// 継承元の型名。
		/// The names of the base types.
		/// </summary>
		public string[] BaseNames{
			get{return base_names;}
		}

		public TypeDefinition[] Bases{get; internal set;}

		/// <summary>
		/// メンバの定義式郡。
		/// The declarations for members.
		/// </summary>
		public Statement[] Body{
			get{return body;}
		}

		/// <summary>
		/// Variable corresponding to the class name
		/// </summary>
		internal ExpressoVariable ExpressoVariable{
			get; set;
		}

        public override NodeType Type
        {
            get { return NodeType.TypeDef; }
        }

		public TypeDefinition(string typeName, Statement[] bodyStmts, DeclType type, string[] basesList)
		{
			name = typeName;
			body = bodyStmts;
			base_names = basesList;
			target_type = type;
		}

        public override bool Equals(object obj)
        {
            var x = obj as TypeDefinition;

            if (x == null) return false;

            return this.target_type == x.target_type && this.name == x.name && this.body.Equals(x.body);
        }

        public override int GetHashCode()
        {
            return this.target_type.GetHashCode() ^ this.name.GetHashCode() ^ this.body.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
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
							throw ExpressoOps.InvalidTypeError("Unknown label!");
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
						throw ExpressoOps.InvalidTypeError("A class declaration can not have that type of statements!");
					}
				}else if(decl is FunctionDeclaration){
					var method = (FunctionDeclaration)decl;
					decl_target.Add(method.Name, offset++);
					members.Add(method);
				}else{
					throw ExpressoOps.InvalidTypeError("A class declaration can not have that type of statements!");
				}
			}

			object type_def = null;
			switch(TargetType){
			case DeclType.Class:
				type_def = new ClassDefinition(Name, privates, publics, members.ToArray(), null);
				break;
			}
			return type_def;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				foreach(var stmt in body)
					stmt.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return string.Format("Definition of {0} {1}", target_type, name);
		}

		/*internal override bool NeedsLocalContext{
			get {
				return true;
			}
		}*/
		
		internal override bool ExposesLocalVariable(ExpressoVariable variable)
		{
			return true;
		}
		
		internal override ExpressoVariable BindReference(ExpressoNameBinder binder, ExpressoReference reference)
		{
			ExpressoVariable variable;
			
			// Python semantics: The variables bound local in the class
			// scope are accessed by name - the dictionary behavior of classes
			if (TryGetVariable(reference.Name, out variable)) {
				// TODO: This results in doing a dictionary lookup to get/set the local,
				// when it should probably be an uninitialized check / global lookup for gets
				// and a direct set
				if (variable.Kind == VariableKind.Global) {
					AddReferencedGlobal(reference.Name);
				} else if (variable.Kind == VariableKind.Local) {
					return null;
				}
				
				return variable;
			}
			
			// Try to bind in outer scopes, if we have an unqualified exec we need to leave the
			// variables as free for the same reason that locals are accessed by name.
			for (ScopeStatement parent = Parent; parent != null; parent = parent.Parent) {
				if (parent.TryBindOuter(this, reference, out variable)) {
					return variable;
				}
			}
			
			return null;
		}
	}
}

