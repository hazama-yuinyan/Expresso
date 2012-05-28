using System.Collections.Generic;

namespace Expresso.Ast
{
    /// <summary>
    /// 抽象構文木のノードタイプ。
    /// </summary>
    public enum NodeType
    {
        Block,
        UnaryExpression,
        BinaryExpression,
        ConditionalExpression,
        Constant,
        Parameter,
        Call,
        Assignment,
        Function,
        Return,
		Print,
		Range,
		VarDecl,
		ExprStatement,
		IfStatement,
		WhileStatement,
		ForStatement,
		AssertStatement,
		TryStatement
    }

    /// <summary>
    /// 抽象構文木のノードの共通基底。
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// ノートタイプ。
        /// </summary>
        public abstract NodeType Type { get; }

        internal protected Node() { }

        #region 抽象構文木から仮想マシン語コードを生成。

        /// <summary>
        /// operator== とかをオーバーロードしてるクラスでも、参照に基づいてDictionaryとかを使うために。
        /// </summary>
        class ReferenceEqual<T> : IEqualityComparer<T>
        {
            bool IEqualityComparer<T>.Equals(T x, T y)
            {
                return object.ReferenceEquals(x, y);
            }

            int IEqualityComparer<T>.GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }

        /// <summary>
        /// 抽象構文木をコンパイルして、仮想マシン語コードを得る。
        /// </summary>
        /// <returns>コード列。</returns>
        public IEnumerable<Emulator.Instruction> Compile()
        {
            var localTable = new Dictionary<Parameter, int>();
            var addressTable = new Dictionary<Function, int>(new ReferenceEqual<Function>());
            var functionTable = new Dictionary<Function, IEnumerable<Emulator.Instruction>>(new ReferenceEqual<Function>());

            var main = this.Compile(localTable, addressTable, functionTable);

            var list = new List<Expresso.Emulator.Instruction>(main);
            list.Add(Expresso.Emulator.Instruction.Halt);

            foreach (var f in functionTable)
            {
                addressTable[f.Key] = list.Count;
                list.AddRange(f.Value);
            }

            return list;
        }

        /// <summary>
        /// コンパイル作業の本体。
        /// </summary>
        /// <remarks>
        /// Compile() の方では、関数ごとに得られたコードをつないで、関数のアドレス解決をしてるだけ。
        /// </remarks>
        /// <param name="localTable">ローカル変数テーブル。どの識別子が何番目のローカル変数か。</param>
        /// <param name="addressTable">関数アドレステーブル。アドレス解決は後からするけど、とりあえずテーブルの参照だけ渡しておく。</param>
        /// <param name="functionTable">関数テーブル。関数のコンパイル結果はいったんこのテーブルに格納しておく。Compile()内でつなぐ。</param>
        /// <returns>部分木のコンパイル結果。</returns>
        protected internal abstract IEnumerable<Emulator.Instruction> Compile(
            Dictionary<Parameter, int> localTable,
            Dictionary<Function, int> addressTable,
            Dictionary<Function, IEnumerable<Emulator.Instruction>> functionTable);

        #endregion
        #region 生成関数群

        public static Parameter Parameter(string name)
        {
            return new Parameter { Name = name };
        }
		
		public static UnaryExpression Negate(Expression expr)
		{
			return Unary(OperatorType.MINUS, expr);
		}

        public static UnaryExpression Unary(OperatorType op, Expression ex)
        {
            return new UnaryExpression { Operator = op, Operand = ex};
        }

        static BinaryExpression Binary(OperatorType op, Expression l, Expression r)
        {
            return new BinaryExpression { Operator = op, Left = l, Right = r };
        }

        public static BinaryExpression Add(Expression l, Expression r) { return Binary(OperatorType.PLUS, l, r); }
        public static BinaryExpression Subtract(Expression l, Expression r) { return Binary(OperatorType.MINUS, l, r); }
        public static BinaryExpression Multiply(Expression l, Expression r) { return Binary(OperatorType.TIMES, l, r); }
        public static BinaryExpression Divide(Expression l, Expression r) { return Binary(OperatorType.DIV, l, r); }
		public static BinaryExpression Power(Expression l, Expression r){return Binary(OperatorType.POWER, l, r);}
        public static BinaryExpression LessThan(Expression l, Expression r) { return Binary(OperatorType.LESS, l, r); }
        public static BinaryExpression LessEqual(Expression l, Expression r) { return Binary(OperatorType.LESE, l, r); }
        public static BinaryExpression GreaterThan(Expression l, Expression r) { return Binary(OperatorType.GREAT, l, r); }
        public static BinaryExpression GreaterEqual(Expression l, Expression r) { return Binary(OperatorType.GRTE, l, r); }
        public static BinaryExpression Equal(Expression l, Expression r) { return Binary(OperatorType.EQUAL, l, r); }
        public static BinaryExpression NotEqual(Expression l, Expression r) { return Binary(OperatorType.NOTEQ, l, r); }
        public static BinaryExpression And(Expression l, Expression r) { return Binary(OperatorType.AND, l, r); }
        public static BinaryExpression Or(Expression l, Expression r) { return Binary(OperatorType.OR, l, r); }

        public static Block Block(params Statement[] statements)
        {
            var block = new Block();
            block.Statements.AddRange(statements);
            return block;
        }

        #endregion
    }
}
