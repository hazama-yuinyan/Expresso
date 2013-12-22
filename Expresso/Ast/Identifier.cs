using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Compiler.Meta;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// 識別子。
	/// Reperesents a symbol.
    /// </summary>
    public class Identifier : Assignable
    {
		readonly string name;

        /// <summary>
        /// 識別子名。
		/// The name of the identifier.
        /// </summary>
        public string Name{
			get{return name;}
		}

		/// <summary>
		/// 変数の評価スタック内でのオフセット値。
		/// The offset of the variable in the evaluation stack.
		/// </summary>
		public int Offset{
			get{
				return (Reference != null && Reference.Variable != null) ? Reference.Variable.Offset : -1;
			}
		}

		internal ExpressoReference Reference{get; set;}

		/// <summary>
		/// Indicates whether the identifier is resolved(whether the name is bound to a value).
		/// </summary>
		internal bool IsResolved{
			get{return Reference != null && Reference.IsBound;}
		}

		/// <summary>
		/// 変数の型。
		/// The type of the variable.
		/// </summary>
		public TypeAnnotation ParamType{
			get{
				return (Reference != null) ? Reference.VariableType : TypeAnnotation.VoidType;
			}
		}

		/// <summary>
		/// Indicates whether the identifier is definitely assigned to a value.
		/// </summary>
		internal bool Assigned{get; set;}

        public override NodeType Type{
            get{return NodeType.Identifier;}
        }

		internal Identifier(string identName, ExpressoReference reference)
		{
			name = identName;
			Reference = reference;
		}

        public override bool Equals(object obj)
        {
            var x = obj as Identifier;

            if(x == null)
                return false;

            return this.name == x.name;
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			if(ParamType.ObjType == ObjectTypes._SUBSCRIPT)
				return this;
			else if(ParamType.ObjType == ObjectTypes.TYPE_CLASS){
				var cur_module = varStore.Get(0) as ExpressoObj;
				if(cur_module == null)
					throw ExpressoOps.ReferenceError("\"this\" doesn't refer to the enclosing module instance.");

				return cur_module.AccessMember(this, true);
			}else if(ParamType.ObjType == ObjectTypes.TYPE_MODULE){
				var module = ExpressoModule.GetModule(Name);
				if(module == null)
					throw ExpressoOps.MissingTypeError(string.Format("The requested module \"{0}\" doesn't exist.", Name));

				return module;
			}else
				return varStore.Get(Offset, Level);
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){}
			walker.PostWalk(this);
		}

		internal override void Assign(EvaluationFrame frame, object val)
		{
			if(Reference != null)
				frame.Assign(this.Offset, val);
			else
				throw ExpressoOps.RuntimeError("Unbound name: {0}", name);
		}

		public override string ToString()
		{
			return (Reference != null) ? Reference.ToString() : string.Format("[Unbound name: {0}]", name);
		}
    }
}
