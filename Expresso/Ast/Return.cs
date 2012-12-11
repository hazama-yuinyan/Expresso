using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
    /// <summary>
    /// リターン文。
	/// The Return statement.
    /// </summary>
    public class ReturnStatement : Statement
    {
        /// <summary>
        /// 戻り値の式。
        /// </summary>
        public List<Expression> Expressions { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.Return; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ReturnStatement;

            if (x == null) return false;

            return this.Expressions.Equals(x.Expressions);
        }

        public override int GetHashCode()
        {
            return this.Expressions.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
            if(Expressions.Count == 0){
				return new ExpressoTuple(new List<object>());
			}else if(Expressions.Count == 1){
				return Expressions[0].Run(varStore);
			}else{
				var objs = new List<object>();
				foreach (Expression expr in Expressions)
					objs.Add(expr.Run(varStore));
				
				return ExpressoFunctions.MakeTuple(objs);
			}
        }

		internal override System.Linq.Expressions.Expression Compile(Emitter emitter)
		{
			return emitter.Emit(this);
		}
    }
}
