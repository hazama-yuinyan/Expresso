using System;
using System.Collections.Generic;
using Expresso.BuiltIns;

namespace Expresso.Ast
{
	public class ObjectInitializer : Expression
	{
		/// <summary>
        /// オブジェクト生成に使用する式群。
        /// </summary>
        public List<Expression> InitializeList { get; internal set; }
		
		/// <summary>
		/// この式群を評価した結果生成されるオブジェクトのタイプ。
		/// </summary>
		public TYPES ObjType{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.Initializer; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as ObjectInitializer;

            if (x == null) return false;
			
			return InitializeList.Equals(x.InitializeList);
        }

        public override int GetHashCode()
        {
            return this.InitializeList.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            return null;
        }
	}
}

