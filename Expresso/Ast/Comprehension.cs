using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;


namespace Expresso.Ast
{
	public class Comprehension : Expression
	{
		public Expression Body{get; internal set;}

		public Expression Child{get; internal set;}

		public override NodeType Type
        {
            get { return NodeType.Comprehension; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Comprehension;

            if (x == null) return false;

            return this.Body.Equals(x.Body) && this.Child.Equals(x.Child);
        }

        public override int GetHashCode()
        {
            return this.Body.GetHashCode() ^ this.Child.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			var child_store = new VariableStore{Parent = varStore};
			Child.Run(child_store);
			return Body.Run(child_store);
        }
	}

	public class ComprehensionFor : Expression
	{
		/// <summary>
        /// コンテナを走査する式。
        /// The body that will be executed.
        /// </summary>
        public Expression Iteration { get; internal set; }

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public Expression Body{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.ComprehensionFor; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ComprehensionFor;

            if (x == null) return false;

            return this.Iteration.Equals(x.Iteration) && this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Iteration.GetHashCode() ^ this.Body.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			Iteration.Run(varStore);
			return Body.Run(varStore);
        }
	}

	public class ComprehensionIf : Expression
	{
		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
        public Expression Condition { get; internal set; }

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public Expression Body{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.ComprehensionIf; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ComprehensionIf;

            if (x == null) return false;

            return this.Body.Equals(x.Body) && this.Condition.Equals(x.Condition);
        }

        public override int GetHashCode()
        {
            return this.Body.GetHashCode() ^ this.Condition.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			var cond = Condition.Run(varStore);
			if(!(cond is bool))
				throw new EvalException("Cannot evaluate the expression to a boolean.");

			return Body.Run(varStore);
        }
	}
}

