using System;
using System.Collections.Generic;

namespace Expresso.Ast
{
	/// <summary>
	/// For文。
	/// </summary>
	public class ForStatement : Statement
	{
		/// <summary>
        /// 条件式。
        /// </summary>
        public Expression Condition { get; internal set; }

        /// <summary>
        /// 操作する対象の式群。
        /// </summary>
        public List<Expression> Targets { get; internal set; }

        /// <summary>
        /// 操作対象のオブジェクトが存在する間評価し続けるブロック。
        /// </summary>
        public Block Body { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.ForStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ForStatement;

            if (x == null) return false;
			
			return true;
        }

        public override int GetHashCode()
        {
            return this.Condition.GetHashCode() ^ this.Targets.GetHashCode() ^ this.Body.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            return null;
        }
	}
}

