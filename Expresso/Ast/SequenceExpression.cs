using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// カンマで区切られたリテラル式をあらわす。
	/// Reperesents a comma-separated list expression like "1,2,3,4".
	/// It could show up in a simultaneous assignment like "a, b = 1, 2"
    /// </summary>
    public class SequenceExpression : Expression
    {
		readonly Expression[] items;

        /// <summary>
        /// リストのアイテム。
		/// The items of this list.
        /// </summary>
        public Expression[] Items{
			get{return items;}
		}

		public int Count{
			get{return items.Length;}
		}

        public override NodeType Type{
            get{return NodeType.Sequence;}
        }

		public SequenceExpression(Expression[] targetItems)
		{
			items = targetItems;
		}

        public override bool Equals(object obj)
        {
            var x = obj as SequenceExpression;

            if(x == null)
                return false;

            return this.items == x.items;
        }

        public override int GetHashCode()
        {
            return this.items.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			var type_def = TargetDecl.Run(varStore) as BaseDefinition;
			if(type_def == null)
				throw ExpressoOps.ReferenceError("{0} doesn't refer to a type name.", TargetDecl);

			return ExpressoObj.CreateInstance(type_def, Arguments, varStore);
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(AstWalker walker)
		{
			if(walker.Walk(this)){
				foreach(var e in items)
					e.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			for(int i = 0; i < items.Length; ++i){
				if(i != 0)
					sb.Append(",");

				sb.Append(items[i]);
			}
			return sb.ToString();
		}
    }
}
