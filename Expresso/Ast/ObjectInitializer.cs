using System;
using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;

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

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
            ExpressoObj result = null;
			switch (ObjType) {
			case TYPES.TUPLE:
			{
				var tmp_list = new List<ExpressoObj>();
				foreach (var item in InitializeList) {
					tmp_list.Add((ExpressoObj)item.Run(varStore, funcTable));
				}
				result = ExpressoFunctions.MakeTuple(tmp_list);
				break;
			}
				
			case TYPES.LIST:
			{
				var tmp_list = new List<object>();
				foreach (var item in InitializeList) {
					tmp_list.Add(item.Run(varStore, funcTable));
				}
				result = ExpressoFunctions.MakeList(tmp_list);
				break;
			}
				
			case TYPES.DICT:
			{
				var key_list = new List<ExpressoObj>();
				var value_list = new List<object>();
				for (int i = 0; i < InitializeList.Count; ++i) {
					if(i % 2 == 0)
						key_list.Add((ExpressoObj)InitializeList[i].Run(varStore, funcTable));
					else
						value_list.Add(InitializeList[i].Run(varStore, funcTable));
				}
				result = ExpressoFunctions.MakeDict(key_list, value_list, key_list.Count);
				break;
			}
				
			default:
				throw new EvalException("Unknown type of initializer");
			}
			
			return result;
        }
	}
}

