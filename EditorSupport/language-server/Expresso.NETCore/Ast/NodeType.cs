using System;


namespace Expresso.Ast
{
    /// <summary>
    /// 抽象構文木のノードタイプ。
    /// The node type of AST.
    /// </summary>
    public enum NodeType
    {
        Unknown,
        /// <summary>
        /// AstType
        /// </summary>
        TypeReference,
        /// <summary>
        /// Type or delegate declaration.
        /// </summary>
        TypeDeclaration,
        Member,
        Statement,
        Expression,
        Token,
        /// <summary>
        /// Comment or whitespace
        /// </summary>
        Whitespace,
        /// <summary>
        /// Placeholder for a pattern
        /// </summary>
        Pattern
    }
}
