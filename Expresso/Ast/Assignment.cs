﻿using System.Collections.Generic;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// 代入文。
	/// The assignment statement.
	/// targets.length == 1 for simple assignments like "x = 1"
    /// and targets.length == 3 for more complex assignments like "x = y = z = 3"
    /// </summary>
    public class Assignment : Statement
    {
		readonly Expression[] targets;
		readonly Expression rhs;

        /// <summary>
        /// 代入先の変数郡。
        /// The target expressions that will be bounded.
        /// </summary>
        public Expression[] Left{
			get{return targets;}
		}

        /// <summary>
        /// 右辺値の式。
		/// The expression that will be assigned.
        /// </summary>
        public Expression Right{
			get{return rhs;}
		}

        public override NodeType Type{
            get{return NodeType.Assignment;}
        }

		public Assignment(Expression[] lhs, Expression rhsExpr)
		{
			targets = lhs;
			rhs = rhsExpr;
		}

        public override bool Equals(object obj)
        {
            var x = obj as Assignment;

            if(x == null)
                return false;

            return this.rhs.Equals(x.rhs)
                && this.targets.Equals(x.targets);
        }

        public override int GetHashCode()
        {
            return this.targets.GetHashCode() ^ this.rhs.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			int i;
			var rvalues = new List<object>(Expressions.Count);
			for(i = 0; i < Expressions.Count; ++i)	//まず右辺をすべて評価する
				rvalues.Add(Expressions[i].Run(varStore));

			for(i = 0; i < Targets.Count; ++i){		//その後左辺値に代入する
				var assignable = Targets[i] as Assignable;
				if(assignable == null)
					throw ExpressoOps.ReferenceError("Can not assign a value to the target!");

				assignable.Assign(varStore, rvalues[i]);
			}
			return rvalues[0];	//x = y = 0;みたいな表記を許容するために右辺値の一番目を戻り値にする
        }*/
		
		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				foreach(var e in targets)
					e.Walk(walker);

				rhs.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach(var target in targets)
				sb.Append(target + " = ");

			sb.Append(rhs);
			return sb.ToString();
		}
    }
}
