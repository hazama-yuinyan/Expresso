using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expresso.BuiltIns;

namespace Expresso.Ast
{
    /// <summary>
    /// 関数定義。
    /// </summary>
    public class Function : Statement
    {
        /// <summary>
        /// 関数名。
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 仮引数リスト。
        /// </summary>
        public List<Parameter> Parameters { get; internal set; }

        /// <summary>
        /// 関数本体。
        /// </summary>
        public Block Body { get; internal set; }
		
		/// <summary>
		/// 戻り値の型
		/// </summary>
		public TYPES ReturnType {get; internal set;}

        /// <summary>
        /// 関数内で定義されたローカル変数一覧。
        /// </summary>
        public IEnumerable<Parameter> LocalVariables
        {
            get
            {
                return this.Body.LocalVariables;
            }
        }

        /// <summary>
        /// 関数＋引数名、
        /// f(x, y) {x + y;} の f(x, y) の部分を文字列として返す。
        /// </summary>
        /// <returns>関数＋引数名</returns>
        public string Signature()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.Name);
            sb.Append("(");

            bool first = true;

            foreach (var param in this.Parameters)
            {
                if (first) first = false;
                else sb.Append(", ");

                sb.Append(param.Name);
            }

            sb.Append(")");

            return sb.ToString();
        }

        public override NodeType Type
        {
            get { return NodeType.Function; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Function;

            if (x == null) return false;

            if (this.Name != x.Name) return false;

            if (this.Parameters.Count != x.Parameters.Count) return false;

            for (int i = 0; i < this.Parameters.Count; i++)
            {
                if (!this.Parameters[i].Equals(x.Parameters[i])) return false;
            }

            return this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Parameters.GetHashCode() ^ this.Body.GetHashCode();
        }

        protected internal override IEnumerable<Expresso.Emulator.Instruction> Compile(Dictionary<Parameter, int> localTable, Dictionary<Function, int> addressTable, Dictionary<Function, IEnumerable<Expresso.Emulator.Instruction>> functionTable)
        {
            return null;
        }
		
		public override string ToString()
		{
			return string.Format("[Function: Name={0}, Parameters={1}, Body={2}, ReturnType={3}, LocalVariables={4}, Type={5}]", Name, Parameters, Body, ReturnType, LocalVariables, Type);
		}
    }
}
