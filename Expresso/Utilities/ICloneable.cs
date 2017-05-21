using System;
namespace Expresso
{
    /// <summary>
    /// Public interface for an cloneable object.
    /// </summary>
    public interface ICloneable<T>
    {
        T Clone();
    }
}
