using System;
using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// 変数宣言式。
	/// The variable declaration.
	/// </summary>
	public class VarDeclaration : Expression
	{
		Identifier[] left;
		Expression[] expressions;

		/// <summary>
		/// 代入先の左辺値の式。
		/// Variables that will be declared.
		/// </summary>
		public Identifier[] Left{
			get{return left;}
		}

        /// <summary>
        /// 右辺値の式。
		/// The right-hand-side expressions. It can contain null elements if the corresponding declarations don't have
		/// initialization.
        /// </summary>
        public Expression[] Expressions{
			get{return expressions;}
		}

        public override NodeType Type
        {
            get { return NodeType.VarDecl; }
        }

		public VarDeclaration(Identifier[] lhs, Expression[] rhs)
		{
			left = lhs;
			expressions = rhs;
		}

        public override bool Equals(object obj)
        {
            var x = obj as VarDeclaration;

            if (x == null) return false;

            return expressions.Equals(x.expressions) && left.Equals(x.left);
        }

        public override int GetHashCode()
        {
            return left.GetHashCode() ^ expressions.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			object obj;
			for (int i = 0; i < Variables.Count; ++i) {		//let x = 0, y = 1;みたいな表記しかできない。let x = y = 0;も、
				obj = Expressions[i].Run(varStore);			//let x, y = 0, 1;も（いまのところ）不可能
				varStore.Assign(Variables[i].Offset, obj);	//せめて前者には対応したいところ
			}
			return null;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				foreach(var lhs in left)
					lhs.Walk(walker);

				foreach(var e in expressions){
					if(e != null)
						e.Walk(walker);
				}
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return string.Format("let {0} : {1}", left, expressions);
		}
	}
}

