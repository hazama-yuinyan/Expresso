using System;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Compiler.Meta;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// Represents an argument(both for parameters, which appear in function signatures, and for actual parameters, which show up
	/// in call expressions).
	/// </summary>
	public class Argument : Expression
	{
        /// <summary>
        /// この引数の名前。
		/// The name of the argument.
        /// </summary>
        public string Name{
			get{
				return (ExpressoVariable != null) ? ExpressoVariable.Name : "<anonymous>";
			}
		}

		/// <summary>
		/// 引数の実体。
		/// The value of the argument.
		/// </summary>
		internal ExpressoVariable ExpressoVariable{get; set;}

		/// <summary>
		/// 引数の評価スタック内でのオフセット値。
		/// The offset of the argument in the evaluation stack.
		/// </summary>
        public int Offset{
			get{
				return (ExpressoVariable != null) ? ExpressoVariable.Offset : -1;
			}
		}

        /// <summary>
        /// この引数のデフォルト値。
		/// The optional value for this argument. It would be null if none is specified.
        /// </summary>
        public Expression Option{get; internal set;}

		/// <summary>
		/// この引数の型。
		/// The type of the argument.
		/// </summary>
        public TypeAnnotation ParamType{
			get{
				return (ExpressoVariable != null) ? ExpressoVariable.ParamType : null;
			}
		}

        public override NodeType Type{
            get{return NodeType.Argument;}
        }

		internal Argument(Expression option, ExpressoVariable variable)
		{
			Option = option;
			ExpressoVariable = variable;
		}

        public override bool Equals(object obj)
        {
            var x = obj as Argument;

            if(x == null)
                return false;

            return ExpressoVariable.Equals(x.ExpressoVariable);
        }

        public override int GetHashCode()
        {
            return ExpressoVariable.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
            return Option;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				if(Option != null)
					Option.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			return (Option != null) ? string.Format("{0} (- {1} [= {2}]", Name, ParamType, Option)
				: string.Format("{0} (- {1}", Name, ParamType);
		}
	}
}

