using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	public class ObjectInitializer : Expression
	{
		/// <summary>
        /// オブジェクト生成に使用する式群。
		/// Expressions generating a sequence object.
        /// </summary>
        public IEnumerable<Expression> Initializer { get; internal set; }
		
		/// <summary>
		/// この式群を評価した結果生成されるオブジェクトのタイプ。
		/// The type of sequence object generated by this node.
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
			
			return Initializer.Equals(x.Initializer);
        }

        public override int GetHashCode()
        {
            return this.Initializer.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
            object result = null;
			switch (ObjType) {
			case TYPES.TUPLE:
			{
				var tmp_list = new List<object>(Initializer.Count());
				foreach (var item in Initializer)
					tmp_list.Add(item.Run(varStore));

				result = ExpressoFunctions.MakeTuple(tmp_list);
				break;
			}
				
			case TYPES.LIST:
			{
				var tmp_list = new List<object>(Initializer.Count());
				foreach (var item in Initializer)
					tmp_list.Add(item.Run(varStore));

				result = ExpressoFunctions.MakeList(tmp_list);
				break;
			}
				
			case TYPES.DICT:
			{
				var len = Initializer.Count();
				var dict_len = len / 2;
				var key_list = new List<object>(dict_len);
				var value_list = new List<object>(dict_len);
				var i = 0;
				foreach(var obj in Initializer){
					if(i % 2 == 0)
						key_list.Add(obj.Run(varStore));
					else
						value_list.Add(obj.Run(varStore));

					++i;
				}
				result = ExpressoFunctions.MakeDict(key_list, value_list);
				break;
			}
				
			default:
				throw new EvalException("Unknown type of initializer");
			}
			
			return result;
        }

		internal override System.Linq.Expressions.Expression Compile(Emitter emitter)
		{
			return emitter.Emit(this);
		}
	}
}

