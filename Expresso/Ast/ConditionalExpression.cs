using System.Collections.Generic;
using System.Linq;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 条件演算。
    /// </summary>
    public class ConditionalExpression : Expression
    {
        /// <summary>
        /// 条件式。
        /// </summary>
        public Expression Condition { get; internal set; }

        /// <summary>
        /// 条件が真の時に返す式。
        /// </summary>
        public Expression TrueExpression { get; internal set; }

        /// <summary>
        /// 条件が偽の時に返す式。
        /// </summary>
        public Expression FalseExpression { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.ConditionalExpression; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ConditionalExpression;

            if (x == null) return false;

            return this.Condition == x.Condition
                && this.TrueExpression.Equals(x.TrueExpression)
                && this.FalseExpression.Equals(x.FalseExpression);
        }

        public override int GetHashCode()
        {
            return this.Condition.GetHashCode() ^ this.TrueExpression.GetHashCode() ^ this.FalseExpression.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
            if((bool)Condition.Run(varStore))
				return TrueExpression.Run(varStore);
			else
				return FalseExpression.Run(varStore);
        }

		public override string ToString ()
		{
			return string.Format("{0} ? {1} : {2}", Condition, TrueExpression, FalseExpression);
		}
    }
}
