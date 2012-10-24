using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 定数。
	/// Represents a constant.
    /// </summary>
    public class Constant : Expression
    {
		/// <summary>
		/// この定数値の型。
		/// The type of this constant.
		/// </summary>
		public TYPES ValType{get; internal set;}
		
        /// <summary>
        /// 定数の値。
		/// The value.
        /// </summary>
        public object Value{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.Constant; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Constant;

            if (x == null) return false;

            return object.Equals(this.Value, x.Value);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			return new ExpressoPrimitive{Value = Value, Type = ValType};
        }

		public override string ToString ()
		{
			return string.Format("{0}", Value);
		}
    }
}
