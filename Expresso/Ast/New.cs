using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
    /// <summary>
    /// オブジェクト生成式。
	/// Reperesents a new expression.
    /// </summary>
    public class NewExpression : Expression
    {
        /// <summary>
        /// オブジェクトを生成する対象のクラス名。
		/// The target class name of the new expression.
        /// </summary>
        public string TargetName { get; internal set; }

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

            return this.TargetName == x.TargetName;
        }

        public override int GetHashCode()
        {
            return this.TargetName.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			return ExpressoClass.CreateInstance(TargetName, Arguments, varStore);
        }

		internal override System.Linq.Expressions.Expression Compile(Emitter emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString()
		{
			return string.Format("new {0} with {1}", TargetName, Arguments);
		}
    }
}
