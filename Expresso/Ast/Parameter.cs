﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 変数。
    /// </summary>
    public class Parameter : Expression
    {
        /// <summary>
        /// 変数名。
        /// </summary>
        public string Name { get; internal set; }
		
		public TYPES ParamType{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.Parameter; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Parameter;

            if (x == null) return false;

            return this.Name == x.Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            if(varStore == null)
				throw new EvalException("Can not find variable store");
				
			return varStore.Get(Name);
        }
    }
}
