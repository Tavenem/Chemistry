using System;
using System.Collections.Generic;
using Tavenem.HugeNumbers;
using Tavenem.Mathematics.HugeNumbers;

namespace Tavenem.Chemistry.HugeNumbers
{
    /// <summary>
    /// A physical object with a size and shape, temperature, and overall density. It may or may not
    /// also have a particular chemical composition.
    /// </summary>
    [JsonInterfaceConverter(typeof(IMaterialConverter))]
    public interface IMaterial : ICloneable, IEquatable<IMaterial>
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
        HugeNumber Mass { get; set; }

        /// <summary>
        /// <para>
        /// The position of this <see cref="IMaterial"/>.
        /// </para>
        /// <para>
        /// A convenience property which gets the <see cref="IShape.Position"/> property of <see
        /// cref="Shape"/>, and replaces <see cref="Shape"/> with a clone at the new position upon
        /// setting a new value.
        /// </para>
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// <para>
        /// The rotation of this <see cref="IMaterial"/>.
        /// </para>
        /// <para>
        /// A convenience property which gets the <see cref="IShape.Rotation"/> property of <see
        /// cref="Shape"/>, and replaces <see cref="Shape"/> with a clone at the new rotation upon
        /// setting a new value.
        /// </para>
        /// </summary>
        Quaternion Rotation { get; set; }

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
        IShape Shape { get; set; }

        /// <summary>
        /// The average temperature of this material, in K. May be <see langword="null"/>,
        /// indicating that it is at the ambient temperature of its environment.
        /// </summary>
        double? Temperature { get; set; }

        /// <summary>
        /// Gets a deep clone of this <see cref="IMaterial"/> instance, optionally at a different
        /// mass.
        /// </summary>
        /// <param name="massFraction">
        /// <para>
        /// The proportion of this instance's mass to assign to the clone.
        /// </para>
        /// <para>
        /// Values less than result in <see cref="Material.Empty"/> being returned.
        /// </para>
        /// </param>
        /// <returns>A deep clone of this instance, optionally with a different mass.</returns>
        IMaterial GetClone(decimal massFraction = 1);

        /// <summary>
        /// <para>
        /// In composites, gets the first layer.
        /// </para>
        /// <para>
        /// In other materials, gets the material itself.
        /// </para>
        /// </summary>
        /// <returns>The first layer of a composite, or the material itself.</returns>
        IMaterial GetCore();

        /// <summary>
        /// <para>
        /// In heterogeneous composites, gets a homogenized version of the mixture.
        /// </para>
        /// <para>
        /// In other materials, gets the material itself.
        /// </para>
        /// </summary>
        /// <returns>A homogenized version of a heterogeneous composites, or the material
        /// itself.</returns>
        IMaterial GetHomogenized();

        /// <summary>
        /// <para>
        /// In composites, gets the last layer.
        /// </para>
        /// <para>
        /// In other materials, gets the material itself.
        /// </para>
        /// </summary>
        /// <returns>The last layer of a composite, or the material itself.</returns>
        IMaterial GetSurface();

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
        /// A <see cref="Composite"/> whose components each have the same composition as this
        /// instance, with the specified proportions.
        /// </returns>
        /// <remarks>If the given <paramref name="proportions"/> do not sum to 1, they are
        /// normalized.</remarks>
        IMaterial Split(params decimal[] proportions);
    }
}
