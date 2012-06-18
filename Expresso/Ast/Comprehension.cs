using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;


namespace Expresso.Ast
{
	public class ComprehensionFor : Expression
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
            get { return NodeType.ComprehensionFor; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ComprehensionFor;

            if (x == null) return false;

            return this.Condition.Equals(x.Condition) && this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Condition.GetHashCode() ^ this.Body.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
			ExpressoPrimitive cond;
			while((cond = Condition.Run(varStore, funcTable) as ExpressoPrimitive) != null && (bool)cond.Value){
				Body.Run(varStore, funcTable);
			}
			return null;
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

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
			var cond = Condition.Run(varStore, funcTable) as ExpressoPrimitive;
			if(cond == null)
				throw new EvalException("Cannot evaluate the expression as a boolean.");

			return Body.Run(varStore, funcTable);
        }
	}
}

