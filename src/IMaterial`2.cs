namespace Tavenem.Chemistry;

/// <summary>
/// A physical object with a size and shape, temperature, and overall density. It may or may not
/// also have a particular chemical composition.
/// </summary>
public interface IMaterial<TSelf, TScalar> : IMaterial<TScalar>
    where TSelf : IMaterial<TSelf, TScalar>
    where TScalar : IFloatingPoint<TScalar>
{
    /// <summary>
    /// Gets a deep clone of this <see cref="IMaterial{TSelf, TScalar}"/> instance.
    /// </summary>
    /// <returns>A deep clone of this instance.</returns>
    TSelf GetTypedClone();
}
