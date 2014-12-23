using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Compiler;

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
        /// <summary>
        /// リストのアイテム。
		/// The items of this list.
        /// </summary>
        public AstNodeCollection<Expression> Items{
            get{return GetChildrenByRole(Roles.TargetExpression);}
		}

		public int Count{
            get{return Items.Count();}
		}

        public override NodeType NodeType{
            get{return NodeType.Expression;}
        }

        public SequenceExpression(IEnumerable<Expression> targetItems)
		{
            foreach(var item in targetItems)
                AddChild(item, Roles.TargetExpression);
		}

        public override bool Equals(object obj)
        {
            var x = obj as SequenceExpression;

            if(x == null)
                return false;

            return this.Items == x.Items;
        }

        public override int GetHashCode()
        {
            return this.Items.GetHashCode();
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitSequence(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSequence(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitSequence(this, data);
        }

		public override string ToString()
		{
			var sb = new StringBuilder();
            for(int i = 0; i < Items.Count(); ++i){
				if(i != 0)
					sb.Append(",");

                sb.Append(Items[i]);
			}
			return sb.ToString();
		}
    }
}
