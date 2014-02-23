using System;


namespace Expresso.Ast
{
    /// <summary>
    /// メソッドやフィールド変数などの属性を表す。
    /// Indicates accesibility or property of the associated fields. 
    /// </summary>
    [Flags]
    public enum Flags{
        None = 0x00,
        PrivateAccess,
        ProtectedAccess,
        PublicAccess,
        StaticMember
    }

    /// <summary>
    /// 対象のオブジェクトに対して属性指定可能なことを宣言する。
    /// Denotes that the inherited classes can be "flag-enabled".
    /// </summary>
    public interface IFlaggable
    {
        Flags Flag{get; set;}
        bool HasFlag(Flags flag);
    }
}

