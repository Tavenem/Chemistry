using System.Numerics;
using System.Text.Json.Serialization;
using Tavenem.Mathematics;

namespace Tavenem.Chemistry;

/// <summary>
/// A physical object with a size and shape, temperature, and overall density. It may or may not
/// also have a particular chemical composition.
/// </summary>
[JsonConverter(typeof(MaterialConverterFactory))]
public interface IMaterial<TScalar> : ICloneable, IEquatable<IMaterial<TScalar>> where TScalar : IFloatingPointIeee754<TScalar>
{
    /// <summary>
    /// This material's constituent substances.
    /// </summary>
    IReadOnlyDictionary<ISubstanceReference, decimal> Constituents { get; }

    /// <summary>
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </summary>
    double Density { get; set; }

    /// <summary>
    /// Whether this material is an empty instance.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// The mass of this material, in kg.
    /// </summary>
    TScalar Mass { get; set; }

    /// <summary>
    /// <para>
    /// The position of this <see cref="IMaterial{TScalar}"/>.
    /// </para>
    /// <para>
    /// A convenience property which gets the <see cref="IShape{TScalar}.Position"/> property of <see
    /// cref="Shape"/>, and replaces <see cref="Shape"/> with a clone at the new position upon
    /// setting a new value.
    /// </para>
    /// </summary>
    Vector3<TScalar> Position { get; set; }

    /// <summary>
    /// <para>
    /// The rotation of this <see cref="IMaterial{TScalar}"/>.
    /// </para>
    /// <para>
    /// A convenience property which gets the <see cref="IShape{TScalar}.Rotation"/> property of <see
    /// cref="Shape"/>, and replaces <see cref="Shape"/> with a clone at the new rotation upon
    /// setting a new value.
    /// </para>
    /// </summary>
    Quaternion<TScalar> Rotation { get; set; }

    /// <summary>
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </summary>
    IShape<TScalar> Shape { get; set; }

    /// <summary>
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </summary>
    double? Temperature { get; set; }

    /// <summary>
    /// Adds the given <paramref name="substance"/> as a new constituent of this material, in the
    /// given <paramref name="proportion"/>.
    /// </summary>
    /// <param name="substance">An <see cref="ISubstance"/> to add.</param>
    /// <param name="proportion">
    /// The proportion at which to add the <paramref name="substance"/>.
    /// </param>
    /// <returns>This instance.</returns>
    IMaterial<TScalar> Add(ISubstanceReference substance, decimal proportion = 0.5m);

    /// <summary>
    /// Adds the given <paramref name="substance"/> as a new constituent of this material, in the
    /// given <paramref name="proportion"/>.
    /// </summary>
    /// <param name="substance">An <see cref="ISubstance"/> to add.</param>
    /// <param name="proportion">
    /// The proportion at which to add the <paramref name="substance"/>.
    /// </param>
    /// <returns>This instance.</returns>
    IMaterial<TScalar> Add(ISubstance substance, decimal proportion = 0.5m);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's composition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    IMaterial<TScalar> Add(IEnumerable<(ISubstanceReference substance, decimal proportion)> constituents);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's composition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    IMaterial<TScalar> Add(IEnumerable<(ISubstance substance, decimal proportion)> constituents);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's composition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    IMaterial<TScalar> Add(params (ISubstanceReference substance, decimal proportion)[] constituents);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's composition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    IMaterial<TScalar> Add(params (ISubstance substance, decimal proportion)[] constituents);

    /// <summary>
    /// Gets a deep clone of this <see cref="IMaterial{TScalar}"/> instance, possibly with a different
    /// mass.
    /// </summary>
    /// <param name="massFraction">
    /// <para>
    /// The proportion of this instance's mass to assign to the clone.
    /// </para>
    /// <para>
    /// Values ≤ 0 result in an empty material being returned.
    /// </para>
    /// </param>
    /// <returns>A deep clone of this instance, possibly with a different mass.</returns>
    IMaterial<TScalar> GetClone(TScalar massFraction);

    /// <summary>
    /// Gets a deep clone of this <see cref="IMaterial{TScalar}"/> instance.
    /// </summary>
    /// <returns>A deep clone of this instance.</returns>
    IMaterial<TScalar> GetClone();

    /// <summary>
    /// <para>
    /// In composites, gets the first layer.
    /// </para>
    /// <para>
    /// In other materials, gets the material itself.
    /// </para>
    /// </summary>
    /// <returns>The first layer of a composite, or the material itself.</returns>
    IMaterial<TScalar> GetCore();

    /// <summary>
    /// <para>
    /// In heterogeneous composites, gets a homogenized version of the mixture.
    /// </para>
    /// <para>
    /// In other materials, gets the material itself.
    /// </para>
    /// </summary>
    /// <returns>
    /// A homogenized version of a heterogeneous composites, or the material itself.
    /// </returns>
    IMaterial<TScalar> GetHomogenized();

    /// <summary>
    /// <para>
    /// In composites, gets the last layer.
    /// </para>
    /// <para>
    /// In other materials, gets the material itself.
    /// </para>
    /// </summary>
    /// <returns>The last layer of a composite, or the material itself.</returns>
    IMaterial<TScalar> GetSurface();

    /// <summary>
    /// <para>
    /// Removes all substances which satisfy the given condition from this material.
    /// </para>
    /// <para>
    /// Has no effect if the substance is not present.
    /// </para>
    /// </summary>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> that defines the conditions of the substances to remove.
    /// </param>
    /// <returns>This instance.</returns>
    IMaterial<TScalar> Remove(Predicate<ISubstance> match);

    /// <summary>
    /// Splits this substance into a composite, whose components have the same composition as
    /// this instance. Each component of the new composite will have the given proportions,
    /// starting from the innermost.
    /// </summary>
    /// <param name="proportions">
    /// <para>
    /// The proportions of the intended components. If only one value is provided, a second is
    /// inferred. If none are provided, the result will be two components in equal proportions.
    /// </para>
    /// <para>
    /// If only a single value is provided, and it is less than or equal to zero, or greater
    /// than or equal to one, this instance is returned unchanged.
    /// </para>
    /// </param>
    /// <returns>
    /// A <see cref="Composite{TScalar}"/> whose components each have the same composition as this
    /// instance, with the specified proportions.
    /// </returns>
    /// <remarks>
    /// If the given <paramref name="proportions"/> do not sum to 1, they are normalized.
    /// </remarks>
    IMaterial<TScalar> Split(params TScalar[] proportions);
}
