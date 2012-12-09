using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expresso.Builtins;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	public abstract class Assignable : Expression
	{
		internal abstract void Assign(VariableStore varStore, object val);
	}

    /// <summary>
    /// 識別子。
	/// Reperesents a symbol.
    /// </summary>
    public class Identifier : Assignable
    {
        /// <summary>
        /// 識別子名。
		/// The name of the identifier.
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
		public TypeAnnotation ParamType{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.Identifier; }
        }

		public Identifier(string name, TypeAnnotation type = null, int offset = -1, int level = 0)
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
			if(ParamType.ObjType == TYPES._SUBSCRIPT)
				return this;
			else if(ParamType.ObjType == TYPES.TYPE_CLASS)
				return null;
			else
				return varStore.Get(Offset, Level);
        }

		internal override void Assign(VariableStore varStore, object val)
		{
			varStore.Assign(Level, Offset, val);
		}

		public override string ToString()
		{
			return string.Format("{0}({2}:{3}) (- {1}", Name, ParamType, Level, Offset);
		}
    }
}
