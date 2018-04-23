namespace Expresso
{
    /// <summary>
    /// Public interface for a cloneable object.
    /// </summary>
    public interface ICloneable<T>
    {
        T Clone();
    }
}
