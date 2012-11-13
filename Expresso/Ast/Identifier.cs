using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 変数。
	/// Reperesents a variable.
    /// </summary>
    public class Identifier : Expression
    {
        /// <summary>
        /// 変数名。
		/// The name of the variable.
        /// </summary>
        public string Name { get; internal set; }

		/// <summary>
		/// 変数の型。
		/// The type of the variable.
		/// </summary>
		public TYPES ParamType{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.Identifier; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Identifier;

            if (x == null) return false;

            return this.Name == x.Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			if(ParamType == TYPES._SUBSCRIPT)
				return this;
			else
				return varStore.Get(Name);
        }

		public override string ToString()
		{
			return string.Format("{0} (- {1}", Name, ParamType);
		}
    }
}
