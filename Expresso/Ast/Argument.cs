using System;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
	public class Argument : Expression
	{
        /// <summary>
        /// この引数の名前。
		/// The name of the argument.
        /// </summary>
        public string Name {get; internal set;}

        /// <summary>
        /// この引数のデフォルト値。
		/// The optional value for this argument. It would be null if none is specified.
        /// </summary>
        public Expression Option {get; internal set;}

		public TYPES ParamType{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.Argument; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Argument;

            if (x == null) return false;

            return this.Name.Equals(x.Name) && this.Option.Equals(x.Option);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Option.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            return Option;
        }
	}
}

