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
		/// 変数の変数ストア内でのオフセット値。
		/// The offset of the variable in the variable store.
		/// </summary>
		public int Offset{get; internal set;}

		/// <summary>
		/// 何階層分親のスコープを辿るか。
		/// The level by which it should track up the scope chain when finding the variable.
		/// </summary>
		public int Level{get; internal set;}

		/// <summary>
		/// 変数の型。
		/// The type of the variable.
		/// </summary>
		public TYPES ParamType{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.Identifier; }
        }

		public Identifier(string name, TYPES type = TYPES.VAR, int offset = -1, int level = 0)
		{
			Name = name;
			ParamType = type;
			Offset = offset;
			Level = level;
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
				return varStore.Get(Offset, Level);
        }

		public override string ToString()
		{
			return string.Format("{0}({2}:{3}) (- {1}", Name, ParamType, Level, Offset);
		}
    }
}
