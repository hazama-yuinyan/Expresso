using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// オブジェクト生成式。
	/// Reperesents a new expression.
    /// </summary>
    public class NewExpression : Expression
    {
        /// <summary>
        /// オブジェクトを生成するクラスの定義を参照する式。
		/// The target class definition of the new expression.
        /// </summary>
        public Expression TargetDecl { get; internal set; }

		/// <summary>
		/// コンストラクタに渡す引数。
		/// The argument list that will be passed to the constructor.
		/// </summary>
		public List<Expression> Arguments{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.New; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as NewExpression;

            if (x == null) return false;

            return this.TargetDecl == x.TargetDecl;
        }

        public override int GetHashCode()
        {
            return this.TargetDecl.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			var type_def = TargetDecl.Run(varStore) as BaseDefinition;
			if(type_def == null)
				throw new EvalException("{0} doesn't refer to a type name.", TargetDecl);

			return ExpressoObj.CreateInstance(type_def, Arguments, varStore);
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString()
		{
			return string.Format("new {0} with {1}", TargetDecl, Arguments);
		}
    }
}
