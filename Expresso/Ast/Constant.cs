using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 定数。
    /// </summary>
    /// <remarks>
    /// 今作ってる言語だと、直定数しかないけども。
    /// </remarks>
    public class Constant : Expression
    {
		public TYPES ValType{get; internal set;}
		
        /// <summary>
        /// 定数の値。
        /// </summary>
        public ExpressoPrimitive Value{get; internal set;}

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

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
			return Value.Value;
        }
    }
}
