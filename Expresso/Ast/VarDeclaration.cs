using System;
using System.Collections.Generic;
using Expresso.Builtins;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// 変数宣言式。
	/// The variable declaration.
	/// </summary>
	public class VarDeclaration : Expression
	{
		/// <summary>
		/// 代入先の左辺値の式。
		/// </summary>
		public List<Identifier> Variables { get; internal set; }

        /// <summary>
        /// 右辺値の式。
        /// </summary>
        public List<Expression> Expressions { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.VarDecl; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as VarDeclaration;

            if (x == null) return false;

            return this.Expressions.Equals(x.Expressions)
                && this.Variables.Equals(x.Variables);
        }

        public override int GetHashCode()
        {
            return this.Variables.GetHashCode() ^ this.Expressions.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			object obj;
			for (int i = 0; i < Variables.Count; ++i) {		//let x = 0, y = 1;みたいな表記しかできない。let x = y = 0;も、
				obj = Expressions[i].Run(varStore);			//let x, y = 0, 1;も（いまのところ）不可能
				varStore.Assign(Variables[i].Offset, obj);	//せめて前者には対応したいところ
			}
			return null;
        }

		public override string ToString()
		{
			return string.Format("let {0} : {1}", Variables, Expressions);
		}
	}
}

