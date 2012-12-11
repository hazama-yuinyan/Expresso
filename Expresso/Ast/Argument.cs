using System;
using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	/// <summary>
	/// Represents an argument.
	/// </summary>
	public class Argument : Expression
	{
		private Identifier ident;

        /// <summary>
        /// この引数の名前。
		/// The name of the argument.
        /// </summary>
        public string Name 
		{
			get{
				return ident.Name;
			}
		}

		/// <summary>
		/// 引数の実体。
		/// The identifier of the argument.
		/// </summary>
		public Identifier Ident
		{
			set{
				ident = value;
			}
		}

		/// <summary>
		/// 引数の変数ストア内でのオフセット値。
		/// The offset of the argument in the variable store.
		/// </summary>
		public int Offset
		{
			get{
				return ident.Offset;
			}
		}

        /// <summary>
        /// この引数のデフォルト値。
		/// The optional value for this argument. It would be null if none is specified.
        /// </summary>
        public Expression Option {get; internal set;}

		/// <summary>
		/// この引数の型。
		/// The type of the argument.
		/// </summary>
		public TypeAnnotation ParamType
		{
			get{
				return ident.ParamType;
			}
		}

        public override NodeType Type
        {
            get { return NodeType.Argument; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Argument;

            if (x == null) return false;

            return ident.Equals(x.ident);
        }

        public override int GetHashCode()
        {
            return ident.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
            return Option;
        }

		internal override System.Linq.Expressions.Expression Compile(Emitter emitter)
		{
			return emitter.Emit(this);
		}

		public override string ToString()
		{
			return (Option != null) ? string.Format("{0} (- {1} [= {2}]", Name, ParamType, Option)
				: string.Format("{0} (- {1}", Name, ParamType);
		}
	}
}

