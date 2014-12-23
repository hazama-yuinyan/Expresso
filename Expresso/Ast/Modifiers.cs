using System;


namespace Expresso.Ast
{
    /// <summary>
    /// メソッドやフィールド変数などの属性を表す。
    /// Indicates accesibility or property of the associated fields. 
    /// </summary>
    [Flags]
    public enum Modifiers
    {
        None = 0x0000,
        Private = 0x0001,
        Protected = 0x0002,
        Public = 0x0004,
        Export = 0x0008,

        Abstract = 0x0010,
        Virtual = 0x0020,
        Static = 0x0040,
        Override = 0x0080,
        Const = 0x0100,

        VisibilityMask = Private | Protected | Public | Export,

        /// <summary>
        /// Special value used to match any modifiers during pattern matching.
        /// </summary>
        Any = unchecked((int)0x80000000)
    }
}

