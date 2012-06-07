using System.Collections.Generic;
using Expresso.BuiltIns;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 抽象構文木のノードタイプ。
	/// The node type of AST.
    /// </summary>
    public enum NodeType
    {
        Block,
        UnaryExpression,
        BinaryExpression,
        ConditionalExpression,
        Constant,
        Parameter,
		Argument,
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
		BreakStatement,
		ContinueStatement,
		AssertStatement,
		TryStatement,
		Initializer,
		StatementList
    }

    /// <summary>
    /// 抽象構文木のノードの共通基底。
    /// The base class for all the abstract syntax trees.
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// ノートタイプ。
		/// The node's type.
        /// </summary>
        public abstract NodeType Type { get; }

        internal protected Node() { }

        #region 抽象構文木を実行する。

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
        /// インタプリターの実行。このノードがあらわす処理を実行する。
        /// Run the code.
        /// </summary>
        /// <param name="varStore">ローカル変数テーブル。</param>
        /// <param name="funcTable">関数テーブル。現在のスコープで参照できる関数の実体を格納してある。</param>
        /// <returns>そのコードを評価した結果の戻り値など。</returns>
        internal abstract object Run(
            VariableStore varStore,
            Scope funcTable);

        #endregion
        #region 生成関数群

        public static Parameter Parameter(string name, TYPES type)
        {
            return new Parameter {Name = name, ParamType = type};
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
