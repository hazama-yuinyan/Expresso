using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// リターン文。
	/// The Return statement.
    /// </summary>
    public class Return : Statement
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
            var x = obj as Return;

            if (x == null) return false;

            return this.Expressions.Equals(x.Expressions);
        }

        public override int GetHashCode()
        {
            return this.Expressions.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            if(Expressions.Count == 0){
				return new ExpressoTuple(new List<ExpressoObj>());
			}else if(Expressions.Count == 1){
				return Expressions[0].Run(varStore, funcTable);
			}else{
				var objs = new List<ExpressoObj>();
				foreach (Expression expr in Expressions) {
					objs.Add((ExpressoObj)expr.Run(varStore, funcTable));
				}
				
				return ExpressoFunctions.MakeTuple(objs);
			}
        }
    }
}
