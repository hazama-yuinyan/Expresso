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
		Sequence,
		Iteration,
		MemRef,
		Comprehension,
		ComprehensionFor,
		ComprehensionIf,
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
		StatementList,
		SwitchStatement,
		CaseClause
    }

    /// <summary>
    /// 抽象構文木のノードの共通基底。
    /// The base class for all the abstract syntax trees.
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// ノードタイプ。
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
        /// <returns>そのコードを評価した結果の戻り値など。</returns>
        internal abstract object Run(VariableStore varStore);
        #endregion
    }
}
