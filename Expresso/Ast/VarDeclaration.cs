using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// 変数宣言式。
	/// </summary>
	public class VarDeclaration : Expression
	{
		/// <summary>
		/// 代入先の左辺値の式。
		/// </summary>
		public List<Parameter> Variables { get; internal set; }

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

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
			ExpressoObj obj;
			for (int i = 0; i < Variables.Count; ++i) {
				obj = (ExpressoObj)Expressions[i].Run(varStore, funcTable);
				varStore.Add(Variables[i].Name, obj);
			}
			return null;
        }
	}
}

