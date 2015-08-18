using System;

using Expresso.Compiler.Meta;

namespace Expresso.Ast
{
    /// <summary>
    /// 名前の参照。
	/// Represents a reference to a name(identifier). An ExpressoReference is created for each name
	/// referred to in a scope (global, class, or function).
    /// </summary>
    class ExpressoReference
    {
		readonly string name;

        /// <summary>
        /// 参照名。
		/// The name of the reference.
        /// </summary>
        public string Name {
			get{return name;}
		}

		public ExpressoVariable Variable{get; set;}

		public TypeAnnotation VariableType{
			get{
				return (Variable != null) ? Variable.ParamType : null;
			}
		}

		public bool IsBound{
			get{
				return Variable != null && Variable.Offset != -1;
			}
		}

		public ExpressoReference(string refName, ExpressoVariable variable = null)
		{
			name = refName;
			Variable = variable;
		}
    }
}
