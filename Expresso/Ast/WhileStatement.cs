using System;
using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Runtime;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/**
	 * While文はインスタンス生成時に全てのメンバーが決定しないので、メンバーは全部変更可能にしておく。
	 */

	/// <summary>
	/// While文.
	/// The While statement.
	/// </summary>
	/// <seealso cref="Node"/>
	/// <seealso cref="BreakableStatement"/>
	public class WhileStatement : BreakableStatement, CompoundStatement
	{
		/// <summary>
        /// 条件式。
		/// The condition.
        /// </summary>
        public Expression Condition{get; internal set;}

        /// <summary>
        /// 条件が真の間評価し続ける文(郡)。
		/// A statement or a block to be processed while the condition is true.
        /// </summary>
        public Statement Body{get; internal set;}

        public override NodeType Type{
            get{return NodeType.WhileStatement;}
        }

		public WhileStatement()
		{
		}

        public override bool Equals(object obj)
        {
            var x = obj as WhileStatement;

            if(x == null)
                return false;

            return Condition == x.Condition && Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return Condition.GetHashCode() ^ Body.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			object cond = null;

			can_continue = true;

			try{
				while(can_continue && (cond = Condition.Run(varStore)) != null && (bool)cond){
					Body.Run(varStore);
				}
			}
			catch(Exception){
				if(!(cond is bool))
					throw ExpressoOps.InvalidTypeError("Invalid expression! The condition of a while statement must yield a boolean!");
			}

			return null;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				Condition.Walk(walker);
				Body.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			return ImplementationHelpers.CollectLocalVars(Body);
		}
	}
}

