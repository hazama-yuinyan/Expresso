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
        /// body内で操作対象となるオブジェクトを参照するのに使用する式群。
        /// 評価結果はlvalueにならなければならない。
        /// </summary>
        public List<Expression> LValues { get; internal set; }

        /// <summary>
        /// 操作する対象の式群。
        /// </summary>
        public List<Expression> Targets { get; internal set; }

        /// <summary>
        /// 操作対象のオブジェクトが存在する間評価し続けるブロック。
        /// </summary>
        public Statement Body { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.ForStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ForStatement;

            if (x == null) return false;
			
			return LValues.Equals(x.LValues) && Targets.Equals(x.Targets) && Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.LValues.GetHashCode() ^ this.Targets.GetHashCode() ^ this.Body.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            return null;
        }
	}
}

