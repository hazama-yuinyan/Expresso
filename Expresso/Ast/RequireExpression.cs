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
    /// モジュールインポート式。
	/// Reperesents a require expression.
    /// </summary>
    public class RequireExpression : Expression
    {
        /// <summary>
        /// 対象となるモジュール名。
		/// The target module name to be required. It can contain a '.'(which can be used to point to a nested module)
		/// or a '/'(which can be used to an external source file).
        /// </summary>
        public string ModuleName { get; internal set; }

		/// <summary>
		/// モジュールに対して与えるエイリアス名。
		/// An alias name that can be used to refer to the module within the scope.
		/// It can be null if none is specified.
		/// </summary>
		public string AliasName{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.Require; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as RequireExpression;

            if (x == null) return false;

            return this.ModuleName == x.ModuleName && this.AliasName == x.AliasName;
        }

        public override int GetHashCode()
        {
            return this.ModuleName.GetHashCode() ^ this.AliasName.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			var path = ModuleName.Replace('.', '/');
			var module_parser = new Parser(new Scanner(path));

			module_parser.ParsingModule.Run(varStore);

			return null;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString()
		{
			return AliasName != null ? string.Format("require {0} as {1}", ModuleName, AliasName) :
				string.Format("require {0}", ModuleName);
		}
    }
}
