using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// オブジェクト生成式。
	/// Reperesents a new expression.
    /// </summary>
    public class NewExpression : Expression
    {
		Expression target;
		Expression[] args;

        /// <summary>
        /// オブジェクトを生成するクラスの定義を参照する式。
		/// The target class definition of the new expression.
        /// </summary>
        public Expression TargetExpr{
			get{return target;}
		}

		/// <summary>
		/// コンストラクタに渡す引数。
		/// The argument list that will be passed to the constructor.
		/// </summary>
		public Expression[] Arguments{
			get{return args;}
		}

        public override NodeType Type
        {
            get { return NodeType.New; }
        }

		public NewExpression(Expression targetExpr, Expression[] arguments)
		{
			target = targetExpr;
			args = arguments;
		}

        public override bool Equals(object obj)
        {
            var x = obj as NewExpression;

            if (x == null) return false;

            return this.target == x.target;
        }

        public override int GetHashCode()
        {
            return this.target.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			var type_def = TargetDecl.Run(varStore) as BaseDefinition;
			if(type_def == null)
				throw ExpressoOps.ReferenceError("{0} doesn't refer to a type name.", TargetDecl);

			return ExpressoObj.CreateInstance(type_def, Arguments, varStore);
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				target.Walk(walker);
				foreach(var arg in args)
					arg.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return string.Format("new {0} with {1}", target, args);
		}
    }
}
