
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 式の共通基底。
    /// </summary>
    public abstract class Expression : Node
    {
    }

	/// <summary>
	/// 左辺式になれるノード。
	/// Represents an assignable node.
	/// </summary>
	public abstract class Assignable : Expression
	{
		internal abstract void Assign(VariableStore varStore, object val);
	}

    /// <summary>
    /// 演算子のタイプ。
    /// </summary>
    public enum OperatorType
    {
		NONE,
        PLUS,
        MINUS,
        TIMES,
        DIV,
		POWER,
		MOD,
        LESS,
        GREAT,
        LESE,
        GRTE,
        EQUAL,
        NOTEQ,
		NOT,
        AND,
        OR,
		BIT_OR,
		BIT_AND,
		BIT_XOR,
		BIT_LSHIFT,
		BIT_RSHIFT
    }
}
