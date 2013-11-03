using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Compiler.Meta;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	internal enum VariableKind
	{
		Local,
		Parameter,
		Global,
		Field
	}

    /// <summary>
    /// Expressoの変数をあらわす。
	/// Reperesents a variable in Expresso. Note that an ExpressoVariable can refer to a local variable,
	/// a parameter of a function, a field of a type, a type and even a function or a method.
    /// </summary>
    internal class ExpressoVariable
    {
		readonly string name;
		readonly TypeAnnotation type;

        /// <summary>
        /// 変数名。
		/// The name of the variable.
        /// </summary>
        public string Name {
			get{return name;}
		}

		/// <summary>
		/// 変数のスタック内でのオフセット値。
		/// The offset of the variable in the call stack.
		/// </summary>
		internal int Offset{get; set;}

		/// <summary>
		/// The scope containing this variable.
		/// </summary>
		public ScopeStatement Scope{get; internal set;}

		/// <summary>
		/// 変数の型。
		/// The type of the variable.
		/// </summary>
		public TypeAnnotation ParamType{
			get{return type;}
		}

		public bool IsGlobal{
			get{return Kind == VariableKind.Global || Scope.IsGlobal;}
		}

		/// <summary>
		/// True if and only if there is a path in control flow graph on which the variable is used before initialized
		/// (declared but not assigned).
		/// </summary>
		public bool ReadBeforeInitialized{
			get; internal set;
		}

		/// <summary>
		/// True if and only if the variable is referred to from the inner scope.(Not used currently)
		/// </summary>
		public bool UsedInNestedScope{
			get; internal set;
		}

		public VariableKind Kind{
			get; internal set;
		}

		public ExpressoVariable(string varName, TypeAnnotation typeAnnot, VariableKind kind, int index = -1,
		                        ScopeStatement scopeStmt = null)
		{
			name = varName;
			Scope = scopeStmt;
			Kind = kind;
			type = typeAnnot;
			Offset = index;
		}
    }
}
