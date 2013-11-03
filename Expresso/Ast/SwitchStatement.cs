using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// Switch文。
	/// The Switch statement.
	/// </summary>
	public class SwitchStatement : Statement, CompoundStatement
	{
		readonly Expression target;
		readonly CaseClause[] cases;

		/// <summary>
        /// 評価対象となる式。
		/// The target expression on which we'll choose which path to go.
        /// </summary>
        public Expression Target{
			get{return target;}
		}

        /// <summary>
        /// 分岐先となるラベル(郡)。
        /// </summary>
        public CaseClause[] Cases{
			get{return cases;}
		}

        public override NodeType Type
        {
            get { return NodeType.SwitchStatement; }
        }

		public SwitchStatement(Expression targetExpr, CaseClause[] caseClauses)
		{
			target = targetExpr;
			cases = caseClauses;
		}

        public override bool Equals(object obj)
        {
            var x = obj as SwitchStatement;

            if (x == null) return false;

            return target == x.target && cases.Equals(x.cases);
        }

        public override int GetHashCode()
        {
            return this.target.GetHashCode() ^ this.cases.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			var target = Target.Run(varStore);

            foreach (var clause in Cases) {
				clause.Target = target;
				if((bool)clause.Run(varStore)) break;
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
				target.Walk(walker);
				foreach(var @case in cases)
					@case.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			return Cases.SelectMany(x => ImplementationHelpers.CollectLocalVars(x.Body));
		}
	}

	/// <summary>
	/// Represents a case clause in switch statements.
	/// </summary>
	public class CaseClause : Expression
	{
		readonly Expression[] labels;
		readonly Statement body;

		/// <summary>
        /// 分岐先となるラベル(郡)。
        /// </summary>
		public Expression[] Labels{
			get{return labels;}
		}

        /// <summary>
        /// 実行対象の文(ブロック)。
		/// The body statement or block.
        /// </summary>
        public Statement Body{
			get{return body;}
		}

        public override NodeType Type
        {
            get { return NodeType.CaseClause; }
        }

		public CaseClause(Expression[] labelExprs, Statement bodyStmt)
		{
			labels = labelExprs;
			body = bodyStmt;
		}

        public override bool Equals(object obj)
        {
            var x = obj as CaseClause;

            if (x == null) return false;

            return this.Labels.Equals(x.Labels)
                && this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Labels.GetHashCode() ^ this.Body.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			return Run(varStore, Target);
        }

		object Run(VariableStore varStore, object target)
		{
			var pt = target;
			bool result = false;

			foreach (var item in Labels) {
				var label_obj = item.Run(varStore);
				var pl = label_obj;
				if(pt is int && label_obj is ExpressoIntegerSequence){
					var int_seq = label_obj as ExpressoIntegerSequence;
					if(int_seq == null)
						throw Expresso.Runtime.Operations.ExpressoOps.InvalidTypeError("Something wrong has occurred!");

					if(int_seq.Includes((int)pt)){
						Body.Run(varStore);
						result = true;
						break;
					}
				}else if(label_obj is Constant && ((Constant)label_obj).ValType == ObjectTypes._CASE_DEFAULT){
					Body.Run(varStore);
					result = true;
					break;
				}else if(pl != null && pl.Equals(pt)){
					Body.Run(varStore);
					result = true;
					break;
				}
			}

			return result;
		}*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				foreach(var label in labels)
					label.Walk(walker);

				body.Walk(walker);
			}
			walker.PostWalk(this);
		}
	}
}

