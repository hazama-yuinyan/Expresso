using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;
using Expresso.Helpers;

namespace Expresso.Ast
{
	/// <summary>
	/// Switch文。
	/// The Switch statement.
	/// </summary>
	public class SwitchStatement : Statement
	{
		/// <summary>
        /// 評価対象となる式。
        /// </summary>
        public Expression Target { get; internal set; }

        /// <summary>
        /// 分岐先となるラベル(郡)。
        /// </summary>
        public List<CaseClause> Cases { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.SwitchStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as SwitchStatement;

            if (x == null) return false;

            return this.Target == x.Target
                && this.Cases.Equals(x.Cases);
        }

        public override int GetHashCode()
        {
            return this.Target.GetHashCode() ^ this.Cases.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			var target = Target.Run(varStore);

            foreach (var clause in Cases) {
				clause.Target = target;
				if((bool)clause.Run(varStore)) break;
            }
			return null;
        }
	}

	/// <summary>
	/// Represents a case clause in switch statements.
	/// </summary>
	public class CaseClause : Expression
	{
		/// <summary>
        /// 分岐先となるラベル(郡)。
        /// </summary>
		public List<Expression> Labels { get; internal set; }

        /// <summary>
        /// 実行対象の文(ブロック)。
		/// The body statement or block.
        /// </summary>
        public Statement Body { get; internal set; }

		/// <summary>
		/// 評価対象となるオブジェクト。
		/// The target object to be evaluated.
		/// </summary>
		public object Target{private get; set;}

        public override NodeType Type
        {
            get { return NodeType.CaseClause; }
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

        internal override object Run(VariableStore varStore)
        {
			return Run(varStore, Target);
        }

		private object Run(VariableStore varStore, object target)
		{
			var pt = target;
			bool result = false;

			foreach (var item in Labels) {
				var label_obj = item.Run(varStore);
				var pl = label_obj;
				if(pt is int && label_obj is ExpressoIntegerSequence){
					var int_seq = label_obj as ExpressoIntegerSequence;
					if(int_seq == null)
						throw new EvalException("Something wrong has occurred!");

					if(int_seq.Includes((int)pt)){
						Body.Run(varStore);
						result = true;
						break;
					}
				}else if(label_obj is Constant && ImplementationHelpers.IsOfType(((Constant)label_obj).ValType, TYPES._CASE_DEFAULT)){
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
		}
	}
}

