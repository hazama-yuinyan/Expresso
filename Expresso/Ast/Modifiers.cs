using System;


namespace Expresso.Ast
{
    /// <summary>
    /// メソッドやフィールド変数などの属性を表す。
    /// Indicates accessibility or property of the associated fields. 
    /// </summary>
    [Flags]
    public enum Modifiers
    {
        /// <summary>
        /// Indicates there is no modifiers applied.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Indicates that the item can only be accessible within the current scope.
        /// </summary>
        Private = 0x0001,

        /// <summary>
        /// Indicates that the item can be accessed from the current and the derived classes.
        /// </summary>
        Protected = 0x0002,

        /// <summary>
        /// Indicates that the item can be accessed from outside of the class.
        /// </summary>
        Public = 0x0004,

        /// <summary>
        /// Indicates that the item can be accessed from outside of the module.
        /// </summary>
        Export = 0x0008,

        /// <summary>
        /// Indicates that the method should be abstract, meaning that the method should not have
        /// the body within the class.
        /// </summary>
        Abstract = 0x0010,

        /// <summary>
        /// Indicates that the method can be overridable in derived classes.
        /// </summary>
        Virtual = 0x0020,

        /// <summary>
        /// Indicates that the item is static through the whole program.
        /// </summary>
        Static = 0x0040,

        /// <summary>
        /// Indicates that the item is overriden in the class.
        /// </summary>
        Override = 0x0080,

        /// <summary>
        /// Indicates that the item can not be modified after once initialized.
        /// </summary>
        Immutable = 0x0100,

        VisibilityMask = Private | Protected | Public | Export,

        /// <summary>
        /// Special value used to match any modifiers during pattern matching.
        /// </summary>
        Any = unchecked((int)0x80000000)
    }
}

