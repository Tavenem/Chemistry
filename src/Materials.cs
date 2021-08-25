using Tavenem.Mathematics;

namespace Tavenem.Chemistry;

/// <summary>
/// Extension methods related to <see cref="IMaterial{TScalar}"/>.
/// </summary>
public static class Materials
{
    /// <summary>
    /// Determines whether this material contains the given <paramref name="substance"/>.
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="substance">An <see cref="ISubstance"/> to test. Both the direct
    /// comstituents of this material, as well as their own constituents (if any) will be
    /// checked for a match.</param>
    /// <returns><see langword="true"/> if this material contains the given <paramref
    /// name="substance"/>; otherwise <see langword="false"/>.</returns>
    public static bool Contains<TScalar>(this IMaterial<TScalar> material, ISubstance substance)
        where TScalar : IFloatingPoint<TScalar>
        => material.Constituents.Any(x => x.Key.Equals(substance))
        || material.Constituents.Any(x => x.Key.Substance.Constituents.Any(y => y.Equals(substance)));

    /// <summary>
    /// Determines whether this material contains the given <paramref name="substance"/>.
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="substance">An <see cref="ISubstance"/> to test. Both the direct
    /// comstituents of this material, as well as their own constituents (if any) will be
    /// checked for a match.</param>
    /// <returns><see langword="true"/> if this material contains the given <paramref
    /// name="substance"/>; otherwise <see langword="false"/>.</returns>
    public static bool Contains<TScalar>(this IMaterial<TScalar> material, ISubstanceReference substance)
        where TScalar : IFloatingPoint<TScalar>
        => material.Constituents.Any(x => x.Key.Equals(substance))
        || material.Constituents.Any(x => x.Key.Substance.Constituents.Any(y => y.Equals(substance)));

    /// <summary>
    /// Determines whether this material contains any constituent which satisfies the given
    /// <paramref name="condition"/>.
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="condition">A <see cref="Predicate{T}"/> of <see cref="ISubstance"/> to test
    /// each constituent of this material, as well as their own constituents (if any).</param>
    /// <returns><see langword="true"/> if this material contains constituent which satisfies
    /// the given <paramref name="condition"/>; otherwise <see langword="false"/>.</returns>
    public static bool Contains<TScalar>(this IMaterial<TScalar> material, Predicate<ISubstance> condition)
        where TScalar : IFloatingPoint<TScalar>
        => material.Constituents.Any(x => condition.Invoke(x.Key.Substance))
        || material.Constituents.Any(x => x.Key.Substance.Constituents.Any(y => condition.Invoke(y.Key.Homogeneous)));

    /// <summary>
    /// Determines whether this material contains the given <paramref name="substance"/>, in the
    /// given phase under the given conditions of <paramref name="temperature"/> and <paramref
    /// name="pressure"/>.
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="substance">An <see cref="ISubstance"/> to test.</param>
    /// <param name="phase">The phase(s) to test. If multiple phases are included, a match will
    /// be counted if the substance is in any of the included phases.</param>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <returns><see langword="true"/> if this material contains the given <paramref
    /// name="substance"/>; otherwise <see langword="false"/>.</returns>
    public static bool Contains<TScalar>(
        this IMaterial<TScalar> material,
        IHomogeneous substance,
        PhaseType phase,
        double temperature,
        double pressure)
        where TScalar : IFloatingPoint<TScalar>
        => material.Constituents.Any(x => x.Key.Substance.Constituents
        .Any(y => y.Key.Equals(substance)
        && (y.Key.Homogeneous.GetPhase(temperature, pressure) & phase) != PhaseType.None));

    /// <summary>
    /// Determines whether this material contains the given <paramref name="substance"/>, in the
    /// given phase under the given conditions of <paramref name="temperature"/> and <paramref
    /// name="pressure"/>.
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="substance">An <see cref="ISubstance"/> to test.</param>
    /// <param name="phase">The phase(s) to test. If multiple phases are included, a match will
    /// be counted if the substance is in any of the included phases.</param>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <returns><see langword="true"/> if this material contains the given <paramref
    /// name="substance"/>; otherwise <see langword="false"/>.</returns>
    public static bool Contains<TScalar>(
        this IMaterial<TScalar> material,
        HomogeneousReference substance,
        PhaseType phase,
        double temperature,
        double pressure)
        where TScalar : IFloatingPoint<TScalar>
        => material.Constituents.Any(x => x.Key.Substance.Constituents
        .Any(y => y.Key.Equals(substance)
        && (y.Key.Homogeneous.GetPhase(temperature, pressure) & phase) != PhaseType.None));

    /// <summary>
    /// Gets a value determined by the individual <see cref="IMaterial{TScalar}.Constituents"/> of
    /// this <see cref="IMaterial{TScalar}"/>, according to their proportions.
    /// </summary>
    /// <typeparam name="TScalar">The type of the <see cref="IMaterial{TScalar}"/></typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="material">This material instance.</param>
    /// <param name="func">A function to extract a value from an <see cref="ISubstance"/>.</param>
    /// <returns>The value.</returns>
    public static TValue GetOverallValue<TScalar, TValue>(this IMaterial<TScalar> material, Func<ISubstance, TValue> func)
        where TScalar : IFloatingPoint<TScalar>
        where TValue : INumber<TValue>
        => material.Constituents.Sum(x => func(x.Key.Substance) * TValue.Create(x.Value));

    /// <summary>
    /// Gets a value determined by the individual <see cref="IMaterial{TScalar}.Constituents"/> of
    /// this <see cref="IMaterial{TScalar}"/>, according to their proportions.
    /// </summary>
    /// <typeparam name="TScalar">The type of the <see cref="IMaterial{TScalar}"/></typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="material">This material instance.</param>
    /// <param name="func">A function to extract a value from an <see cref="ISubstance"/>.</param>
    /// <returns>The value.</returns>
    public static async ValueTask<TValue> GetOverallValueAwaitAsync<TScalar, TValue>(
        this IMaterial<TScalar> material,
        Func<ISubstance, ValueTask<TValue>> func)
        where TScalar : IFloatingPoint<TScalar>
        where TValue : INumber<TValue>
    {
        var sum = TValue.Zero;
        foreach (var (substance, proportion) in material.Constituents)
        {
            sum += await func(substance.Substance).ConfigureAwait(false) * TValue.Create(proportion);
        }
        return sum;
    }

    /// <summary>
    /// Get the proportion of the given <paramref name="constituent"/> in this material.
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="constituent">An <see cref="ISubstance"/> to test. Both the direct
    /// comstituents of this material, as well as their own constituents (if any) will be
    /// checked for a match.</param>
    /// <returns>The overall proportion of the given <paramref name="constituent"/> in this
    /// material, as a value between 0 and 1.</returns>
    public static decimal GetProportion<TScalar>(this IMaterial<TScalar> material, IHomogeneous constituent)
        where TScalar : IFloatingPoint<TScalar>
        => material.Constituents.Sum(x => x.Key.Substance.GetProportion(constituent) * x.Value);

    /// <summary>
    /// Get the proportion of the given <paramref name="constituent"/> in this material.
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="constituent">An <see cref="ISubstance"/> to test. Both the direct
    /// comstituents of this material, as well as their own constituents (if any) will be
    /// checked for a match.</param>
    /// <returns>The overall proportion of the given <paramref name="constituent"/> in this
    /// material, as a value between 0 and 1.</returns>
    public static decimal GetProportion<TScalar>(this IMaterial<TScalar> material, HomogeneousReference constituent)
        where TScalar : IFloatingPoint<TScalar>
        => material.Constituents.Sum(x => x.Key.Substance.GetProportion(constituent) * x.Value);

    /// <summary>
    /// Get the proportion of constituents of this material which satisfy the given
    /// <paramref name="condition"/>.
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="condition">A <see cref="Predicate{T}"/> of <see cref="ISubstance"/> to test
    /// each constituent of this material, as well as their own constituents (if any).</param>
    /// <returns>The overall proportion of constituents of this material which satisfy the given
    /// <paramref name="condition"/>, as a value between 0 and 1.</returns>
    public static decimal GetProportion<TScalar>(this IMaterial<TScalar> material, Predicate<IHomogeneous> condition)
        where TScalar : IFloatingPoint<TScalar>
        => material.Constituents.Sum(x => x.Key.Substance.GetProportion(condition) * x.Value);

    /// <summary>
    /// Calculates the average force of gravity at the surface of this object, in m/s².
    /// </summary>
    /// <param name="material">This instance.</param>
    /// <returns>
    /// The average force of gravity at the surface of this object, in m/s²; or zero if it has
    /// zero <see cref="IMaterial{TScalar}.Mass"/>; or <see cref="double.PositiveInfinity"/> if it has a
    /// radius of zero.
    /// </returns>
    public static TScalar GetSurfaceGravity<TScalar>(this IMaterial<TScalar> material)
        where TScalar : IFloatingPoint<TScalar>
    {
        if (material.Mass.IsNearlyZero())
        {
            return TScalar.Zero;
        }
        if (material.Shape.ContainingRadius.IsNearlyZero())
        {
            return TScalar.PositiveInfinity;
        }
        return NumberValues.GravitationalConstant<TScalar>() * material.Mass / material.Shape.ContainingRadius.Square();
    }

    /// <summary>
    /// <para>
    /// Removes the given substance from this material. Removes the substance from each of its
    /// components, if it is a composite.
    /// </para>
    /// <para>
    /// Has no effect if the substance is not present.
    /// </para>
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="substance">The substance to remove.</param>
    /// <returns>This instance.</returns>
    /// <remarks>
    /// Removes all copies of the substance, if more than one version happens to be present.
    /// </remarks>
    public static IMaterial<TScalar> Remove<TScalar>(this IMaterial<TScalar> material, ISubstance substance)
        where TScalar : IFloatingPoint<TScalar>
        => material.Remove(x => x.Equals(substance));

    /// <summary>
    /// <para>
    /// Removes the given substance from this material. Removes the substance from each of its
    /// components, if it is a composite.
    /// </para>
    /// <para>
    /// Has no effect if the substance is not present.
    /// </para>
    /// </summary>
    /// <param name="material">This material instance.</param>
    /// <param name="substance">The substance to remove.</param>
    /// <returns>This instance.</returns>
    /// <remarks>
    /// Removes all copies of the substance, if more than one version happens to be present.
    /// </remarks>
    public static IMaterial<TScalar> Remove<TScalar>(this IMaterial<TScalar> material, ISubstanceReference substance)
        where TScalar : IFloatingPoint<TScalar>
        => material.Remove(x => x.Equals(substance));

    /// <summary>
    /// Sets this instance's properties to the given values, and updates its shape such that it
    /// is of an appropriate volume.
    /// </summary>
    /// <param name="material">This instance.</param>
    /// <param name="mass">The new mass, in kg.</param>
    /// <param name="density">The new average density, in kg/m³.</param>
    /// <param name="temperature">The temperature at which to determine density, in K.</param>
    /// <param name="pressure">The pressure at which to determine density, in kPa.</param>
    /// <returns>This instance.</returns>
    /// <remarks>
    /// <para>
    /// If any parameter is left <see langword="null"/> its value will be unchanged.
    /// </para>
    /// <para>
    /// If <paramref name="density"/> is left <see langword="null"/> but <paramref
    /// name="temperature"/> and <paramref name="pressure"/> are supplied, the weighted average
    /// density of this instance's constituents under the given conditions of <paramref
    /// name="temperature"/> and <paramref name="pressure"/> is used to make the volume
    /// calculation, without changing the material's assigned density.
    /// </para>
    /// </remarks>
    public static IMaterial<TScalar> ScaleShape<TScalar>(
        this IMaterial<TScalar> material,
        TScalar mass,
        double? density = null,
        double? temperature = null,
        double? pressure = null)
        where TScalar : IFloatingPoint<TScalar>
    {
        material.Mass = mass;
        if (density.HasValue)
        {
            material.Density = density.Value;
        }
        if (!density.HasValue
            && temperature.HasValue
            && pressure.HasValue)
        {
            density = material.GetOverallValue(x => x.GetDensity(temperature.Value, pressure.Value));
        }
        material.Shape = material.Shape
            .GetScaledByVolume(mass / TScalar.Create(density ?? material.Density) / material.Shape.Volume);
        return material;
    }

    /// <summary>
    /// Sets this instance's properties to the given values, and updates its shape such that it
    /// is of an appropriate volume.
    /// </summary>
    /// <param name="material">This instance.</param>
    /// <param name="density">The new average density, in kg/m³.</param>
    /// <param name="temperature">The temperature at which to determine density, in K.</param>
    /// <param name="pressure">The pressure at which to determine density, in kPa.</param>
    /// <returns>This instance.</returns>
    /// <remarks>
    /// <para>
    /// If any parameter is left <see langword="null"/> its value will be unchanged.
    /// </para>
    /// <para>
    /// If <paramref name="density"/> is left <see langword="null"/> but <paramref
    /// name="temperature"/> and <paramref name="pressure"/> are supplied, the weighted average
    /// density of this instance's constituents under the given conditions of <paramref
    /// name="temperature"/> and <paramref name="pressure"/> is used to make the volume
    /// calculation, without changing the material's assigned density.
    /// </para>
    /// </remarks>
    public static IMaterial<TScalar> ScaleShape<TScalar>(
        this IMaterial<TScalar> material,
        double? density = null,
        double? temperature = null,
        double? pressure = null)
        where TScalar : IFloatingPoint<TScalar>
    {
        if (density.HasValue)
        {
            material.Density = density.Value;
        }
        if (!density.HasValue
            && temperature.HasValue
            && pressure.HasValue)
        {
            density = material.GetOverallValue(x => x.GetDensity(temperature.Value, pressure.Value));
        }
        material.Shape = material.Shape
            .GetScaledByVolume(material.Mass / TScalar.Create(density ?? material.Density) / material.Shape.Volume);
        return material;
    }

    /// <summary>
    /// Sets this material's density according to its mass and the volume of its shape.
    /// </summary>
    /// <param name="material">This instance.</param>
    /// <returns>This instance.</returns>
    public static TSelf SetDensityFromMassAndShape<TSelf, TScalar>(this TSelf material)
        where TSelf : IMaterial<TScalar>
        where TScalar : IFloatingPoint<TScalar>
    {
        material.Density = (material.Mass / material.Shape.Volume).Create<TScalar, double>();
        return material;
    }

    /// <summary>
    /// Sets this material's mass given according to its density and the volume of its shape.
    /// </summary>
    /// <param name="material">This instance.</param>
    /// <returns>This instance.</returns>
    public static TSelf SetMassFromDensityAndShape<TSelf, TScalar>(this TSelf material)
        where TSelf : IMaterial<TScalar>
        where TScalar : IFloatingPoint<TScalar>
    {
        material.Mass = TScalar.Create(material.Density) * material.Shape.Volume;
        return material;
    }

    /// <summary>
    /// Alters the position of this material's <see cref="IMaterial{TScalar}.Shape"/>.
    /// </summary>
    /// <param name="material">This instance.</param>
    /// <param name="position">The new position.</param>
    /// <returns>This instance.</returns>
    public static TSelf SetPosition<TSelf, TScalar>(this TSelf material, Vector3<TScalar> position)
        where TSelf : IMaterial<TScalar>
        where TScalar : IFloatingPoint<TScalar>
    {
        material.Shape = material.Shape.GetCloneAtPosition(position);
        return material;
    }

    /// <summary>
    /// Alters the rotation of this material's <see cref="IMaterial{TScalar}.Shape"/>.
    /// </summary>
    /// <param name="material">This instance.</param>
    /// <param name="rotation">The new rotation.</param>
    /// <returns>This instance.</returns>
    public static TSelf SetRotation<TSelf, TScalar>(this TSelf material, Quaternion<TScalar> rotation)
        where TSelf : IMaterial<TScalar>
        where TScalar : IFloatingPoint<TScalar>
    {
        material.Shape = material.Shape.GetCloneWithRotation(rotation);
        return material;
    }
}
