
namespace Expresso.Ast
{
    /// <summary>
    /// 式の共通基底。
    /// </summary>
    public abstract class Expression : Node
    {
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
        AND,
        OR,
		BIT_OR,
		BIT_AND,
		BIT_XOR,
		BIT_LSHIFT,
		BIT_RSHIFT
    }
}
