using System;


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
        Identifier,
        Argument,
        Call,
        Assignment,
        FunctionDef,
        EmptyStatement,
        Return,
        Print,
        IntSequence,
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
        SwitchStatement,
        CaseClause,
        TypeDef,
        Toplevel,
        New,
        Require,
        WithStatement,
        CatchClause,
        FinallyClause,
        ThrowStatement,
        YieldStatement,
        CastExpression,
        IsExpression,
        Sequence,
        DefaultExpression,
        LateBind,
        Pattern,
        Unknown
    }
}
