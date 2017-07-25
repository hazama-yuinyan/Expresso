using System;
namespace Expresso.Runtime.Builtins
{
    /// <summary>
    /// Base interface for all closures in Expresso.
    /// </summary>
    public interface IClosure
    {
        void __Apply();
    }
}
