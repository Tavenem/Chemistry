using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tavenem.HugeNumbers;
using Tavenem.Mathematics.HugeNumbers;

namespace Tavenem.Chemistry
{
    /// <summary>
    /// Extension methods related to <see cref="IMaterial"/>.
    /// </summary>
    public static class Materials
    {
        /// <summary>
        /// Adds the given <paramref name="substance"/> as a new constituent of this <paramref
        /// name="material"/>, in the given <paramref name="proportion"/>. If this <paramref
        /// name="material"/> is a composite, adds the <paramref name="substance"/> to each
        /// component evenly.
        /// </summary>
        /// <param name="material">This <see cref="IMaterial"/> instance.</param>
        /// <param name="substance">An <see cref="ISubstance"/> to add.</param>
        /// <param name="proportion">The proportion at which to add <paramref
        /// name="substance"/>.</param>
        /// <returns>This instance.</returns>
        public static IMaterial AddConstituent(this IMaterial material, ISubstance substance, decimal proportion = 0.5m)
        {
            if (material is Material m)
            {
                m.AddConstituent(substance, proportion);
            }
            else if (material is Composite composite)
            {
                foreach (var component in composite.Components)
                {
                    AddConstituent(component, substance, proportion);
                }
            }
            return material;
        }

        /// <summary>
        /// Adds the given <paramref name="substance"/> as a new constituent of this <paramref
        /// name="material"/>, in the given <paramref name="proportion"/>. If this <paramref
        /// name="material"/> is a composite, adds the <paramref name="substance"/> to each
        /// component evenly.
        /// </summary>
        /// <param name="material">This <see cref="IMaterial"/> instance.</param>
        /// <param name="substance">An <see cref="ISubstance"/> to add.</param>
        /// <param name="proportion">The proportion at which to add <paramref
        /// name="substance"/>.</param>
        /// <returns>This instance.</returns>
        public static IMaterial AddConstituent(this IMaterial material, ISubstanceReference substance, decimal proportion = 0.5m)
        {
            if (material is Material m)
            {
                m.AddConstituent(substance, proportion);
            }
            else if (material is Composite composite)
            {
                foreach (var component in composite.Components)
                {
                    AddConstituent(component, substance, proportion);
                }
            }
            return material;
        }

        /// <summary>
        /// Adds one or more new constituents to this <paramref name="material"/>, at the given
        /// proportions.
        /// </summary>
        /// <param name="material">This <see cref="IMaterial"/> instance.</param>
        /// <param name="constituents">The new constituents to add, as a tuple of a substance and
        /// the proportion to assign to that substance. If a given substance already exists in this
        /// material's conposition, its proportion is adjusted to the given value.</param>
        /// <returns>This instance.</returns>
        public static IMaterial AddConstituents(this IMaterial material, IEnumerable<(ISubstance substance, decimal proportion)> constituents)
        {
            if (material is Material m)
            {
                m.AddConstituents(constituents);
            }
            else if (material is Composite composite)
            {
                foreach (var component in composite.Components)
                {
                    AddConstituents(component, constituents);
                }
            }
            return material;
        }

        /// <summary>
        /// Adds one or more new constituents to this <paramref name="material"/>, at the given
        /// proportions.
        /// </summary>
        /// <param name="material">This <see cref="IMaterial"/> instance.</param>
        /// <param name="constituents">The new constituents to add, as a tuple of a substance and
        /// the proportion to assign to that substance. If a given substance already exists in this
        /// material's conposition, its proportion is adjusted to the given value.</param>
        /// <returns>This instance.</returns>
        public static IMaterial AddConstituents(this IMaterial material, IEnumerable<(ISubstanceReference substance, decimal proportion)> constituents)
        {
            if (material is Material m)
            {
                m.AddConstituents(constituents);
            }
            else if (material is Composite composite)
            {
                foreach (var component in composite.Components)
                {
                    AddConstituents(component, constituents);
                }
            }
            return material;
        }

        /// <summary>
        /// Adds one or more new constituents to this <paramref name="material"/>, at the given
        /// proportions.
        /// </summary>
        /// <param name="material">This <see cref="IMaterial"/> instance.</param>
        /// <param name="constituents">The new constituents to add, as a tuple of a substance and
        /// the proportion to assign to that substance. If a given substance already exists in this
        /// material's conposition, its proportion is adjusted to the given value.</param>
        /// <returns>This instance.</returns>
        public static IMaterial AddConstituents(this IMaterial material, params (ISubstance substance, decimal proportion)[] constituents)
            => AddConstituents(material, constituents.AsEnumerable());

        /// <summary>
        /// Adds one or more new constituents to this <paramref name="material"/>, at the given
        /// proportions.
        /// </summary>
        /// <param name="material">This <see cref="IMaterial"/> instance.</param>
        /// <param name="constituents">The new constituents to add, as a tuple of a substance and
        /// the proportion to assign to that substance. If a given substance already exists in this
        /// material's conposition, its proportion is adjusted to the given value.</param>
        /// <returns>This instance.</returns>
        public static IMaterial AddConstituents(this IMaterial material, params (ISubstanceReference substance, decimal proportion)[] constituents)
            => AddConstituents(material, constituents.AsEnumerable());

        /// <summary>
        /// Determines whether this material contains the given <paramref name="substance"/>.
        /// </summary>
        /// <param name="material">This material instance.</param>
        /// <param name="substance">An <see cref="ISubstance"/> to test. Both the direct
        /// comstituents of this material, as well as their own constituents (if any) will be
        /// checked for a match.</param>
        /// <returns><see langword="true"/> if this material contains the given <paramref
        /// name="substance"/>; otherwise <see langword="false"/>.</returns>
        public static bool Contains(this IMaterial material, ISubstance substance)
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
        public static bool Contains(this IMaterial material, ISubstanceReference substance)
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
        public static bool Contains(this IMaterial material, Predicate<ISubstance> condition)
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
        public static bool Contains(this IMaterial material, IHomogeneous substance, PhaseType phase, double temperature, double pressure)
            => material.Constituents.Any(x => x.Key.Substance.Constituents.Any(y => y.Key.Equals(substance) && (y.Key.Homogeneous.GetPhase(temperature, pressure) & phase) != PhaseType.None));

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
        public static bool Contains(this IMaterial material, HomogeneousReference substance, PhaseType phase, double temperature, double pressure)
            => material.Constituents.Any(x => x.Key.Substance.Constituents.Any(y => y.Key.Equals(substance) && (y.Key.Homogeneous.GetPhase(temperature, pressure) & phase) != PhaseType.None));

        /// <summary>
        /// Gets a value determined by the individual <see cref="IMaterial.Constituents"/> of this
        /// <see cref="IMaterial"/>, according to their proportions.
        /// </summary>
        /// <param name="material">This material instance.</param>
        /// <param name="func">A function to extract a <see cref="double"/> value from an <see
        /// cref="ISubstance"/>.</param>
        /// <returns>The value.</returns>
        public static double GetOverallDoubleValue(this IMaterial material, Func<ISubstance, double> func)
            => material.Constituents.Sum(x => func(x.Key.Substance) * (double)x.Value);

        /// <summary>
        /// Gets a value determined by the individual <see cref="IMaterial.Constituents"/> of this
        /// <see cref="IMaterial"/>, according to their proportions.
        /// </summary>
        /// <param name="material">This material instance.</param>
        /// <param name="func">A function to extract a <see cref="double"/> value from an <see
        /// cref="ISubstance"/>.</param>
        /// <returns>The value.</returns>
        public static async ValueTask<double> GetOverallDoubleValueAwaitAsync(this IMaterial material, Func<ISubstance, ValueTask<double>> func)
        {
            var sum = 0.0;
            foreach (var (substance, proportion) in material.Constituents)
            {
                sum += await func(substance.Substance).ConfigureAwait(false) * (double)proportion;
            }
            return sum;
        }

        /// <summary>
        /// Gets a value determined by the individual <see cref="IMaterial.Constituents"/> of this
        /// <see cref="IMaterial"/>, according to their proportions.
        /// </summary>
        /// <param name="material">This material instance.</param>
        /// <param name="func">A function to extract a <see cref="HugeNumber"/> value from an <see
        /// cref="ISubstance"/>.</param>
        /// <returns>The value.</returns>
        public static HugeNumber GetOverallNumberValue(this IMaterial material, Func<ISubstance, HugeNumber> func)
            => material.Constituents.Sum(x => func(x.Key.Substance) * (HugeNumber)x.Value);

        /// <summary>
        /// Gets a value determined by the individual <see cref="IMaterial.Constituents"/> of this
        /// <see cref="IMaterial"/>, according to their proportions.
        /// </summary>
        /// <param name="material">This material instance.</param>
        /// <param name="func">A function to extract a <see cref="double"/> value from an <see
        /// cref="ISubstance"/>.</param>
        /// <returns>The value.</returns>
        public static async ValueTask<HugeNumber> GetOverallDoubleValueAwaitAsync(this IMaterial material, Func<ISubstance, ValueTask<HugeNumber>> func)
        {
            var sum = HugeNumber.Zero;
            foreach (var (substance, proportion) in material.Constituents)
            {
                sum += await func(substance.Substance).ConfigureAwait(false) * (HugeNumber)proportion;
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
        public static decimal GetProportion(this IMaterial material, IHomogeneous constituent)
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
        public static decimal GetProportion(this IMaterial material, HomogeneousReference constituent)
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
        public static decimal GetProportion(this IMaterial material, Predicate<IHomogeneous> condition)
            => material.Constituents.Sum(x => x.Key.Substance.GetProportion(condition) * x.Value);

        /// <summary>
        /// Calculates the average force of gravity at the surface of this object, in m/s².
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <returns>
        /// The average force of gravity at the surface of this object, in m/s²; or zero if it has
        /// zero <see cref="IMaterial.Mass"/>; or <see cref="HugeNumber.PositiveInfinity"/> if it has a
        /// radius of zero.
        /// </returns>
        public static HugeNumber GetSurfaceGravity(this IMaterial material)
        {
            if (material.Mass.IsZero)
            {
                return HugeNumber.Zero;
            }
            if (material.Shape.ContainingRadius.IsZero)
            {
                return HugeNumber.PositiveInfinity;
            }
            return Mathematics.HugeNumberConstants.G * material.Mass / material.Shape.ContainingRadius.Square();
        }

        /// <summary>
        /// <para>
        /// Removes all substances which satisfy the given condition from this material. Removes
        /// substances from each of the material's components, if it is a composite.
        /// </para>
        /// <para>
        /// Has no effect if the substance is not present.
        /// </para>
        /// </summary>
        /// <param name="material">This material instance.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> that defines the conditions of the
        /// substances to remove.</param>
        /// <returns>This instance.</returns>
        public static IMaterial RemoveConstituents(this IMaterial material, Predicate<ISubstance> match)
        {
            if (material is Material m)
            {
                m.RemoveConstituents(match);
            }
            else if (material is Composite c)
            {
                foreach (var component in c.Components)
                {
                    RemoveConstituents(component, match);
                }
            }
            return material;
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
        public static IMaterial RemoveConstituent(this IMaterial material, ISubstance substance)
            => RemoveConstituents(material, x => x.Equals(substance));

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
        public static IMaterial RemoveConstituent(this IMaterial material, ISubstanceReference substance)
            => RemoveConstituents(material, x => x.Equals(substance));

        /// <summary>
        /// <para>
        /// Sets this instance's mass and density to the given values, and updates its shape such
        /// that it is of an appropriate volume, given those values.
        /// </para>
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <param name="mass">The new mass, in kg.</param>
        /// <param name="density">The new average density, in kg/m³.</param>
        /// <returns>This instance.</returns>
        public static T ScaleShape<T>(this T material, HugeNumber mass, double density) where T : IMaterial
        {
            material.Mass = mass;
            material.Density = density;
            var targetVolume = mass / density;
            material.Shape = material.Shape.ScaleVolume(targetVolume / material.Shape.Volume);
            return material;
        }

        /// <summary>
        /// <para>
        /// Sets this instance's density to the given value, and updates its shape such that it is
        /// of an appropriate volume, given its mass.
        /// </para>
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <param name="density">The new average density, in kg/m³.</param>
        /// <returns>This instance.</returns>
        public static T ScaleShape<T>(this T material, double density) where T : IMaterial
            => material.ScaleShape(material.Mass, density);

        /// <summary>
        /// <para>
        /// Sets this instance's mass to the given value, and updates its shape such that it is of a
        /// volume appropriate to the given <paramref name="mass"/>.
        /// </para>
        /// <para>
        /// The weighted average density of this instance's constituents under the given conditions
        /// of <paramref name="temperature"/> and <paramref name="pressure"/> is used to make the
        /// volume calculation. If the assigned value for density should be used instead, use the
        /// overload which accepts a density parameter, and pass the current value.
        /// </para>
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <param name="mass">The new mass, in kg.</param>
        /// <param name="temperature">The temperature at which to determine density, in K.</param>
        /// <param name="pressure">The pressure at which to determine density, in kPa.</param>
        /// <returns>This instance.</returns>
        public static T ScaleShape<T>(this T material, HugeNumber mass, double temperature, double pressure) where T : IMaterial
        {
            var density = material.GetOverallDoubleValue(x => x.GetDensity(temperature, pressure));
            return material.ScaleShape(mass, density);
        }

        /// <summary>
        /// <para>
        /// Updates this instance's shape such that it is of a volume appropriate to its mass.
        /// </para>
        /// <para>
        /// The weighted average density of this instance's constituents under the given conditions
        /// of <paramref name="temperature"/> and <paramref name="pressure"/> is used to make the
        /// volume calculation. If the assigned value for density should be used instead, use the
        /// overload which accepts a density parameter, and pass the current value.
        /// </para>
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <param name="temperature">The temperature at which to determine density, in K.</param>
        /// <param name="pressure">The pressure at which to determine density, in kPa.</param>
        /// <returns>This instance.</returns>
        public static T ScaleShape<T>(this T material, double temperature, double pressure) where T : IMaterial
            => material.ScaleShape(material.Mass, temperature, pressure);

        /// <summary>
        /// Scales this material's shape according to its mass and density.
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <returns>This instance.</returns>
        public static T ScaleShapeByDensityAndMass<T>(this T material) where T : IMaterial
        {
            material.Shape = material.Shape.ScaleVolume(material.Mass / material.Density / material.Shape.Volume);
            return material;
        }

        /// <summary>
        /// Sets this material's density according to its mass and the volume of its shape.
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <returns>This instance.</returns>
        public static T SetDensityFromMassAndShape<T>(this T material) where T : IMaterial
        {
            material.Density = (double)(material.Mass / material.Shape.Volume);
            return material;
        }

        /// <summary>
        /// Sets this material's mass given according to its density and the volume of its shape.
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <returns>This instance.</returns>
        public static T SetMassFromDensityAndShape<T>(this T material) where T : IMaterial
        {
            material.Mass = material.Density * material.Shape.Volume;
            return material;
        }

        /// <summary>
        /// Alters the position of this material's <see cref="IMaterial.Shape"/>.
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <param name="position">The new position.</param>
        /// <returns>This instance.</returns>
        public static T SetPosition<T>(this T material, Vector3 position) where T : IMaterial
        {
            material.Shape = material.Shape.GetCloneAtPosition(position);
            return material;
        }

        /// <summary>
        /// Alters the rotation of this material's <see cref="IMaterial.Shape"/>.
        /// </summary>
        /// <param name="material">This instance.</param>
        /// <param name="rotation">The new rotation.</param>
        /// <returns>This instance.</returns>
        public static T SetRotation<T>(this T material, Quaternion rotation) where T : IMaterial
        {
            material.Shape = material.Shape.GetCloneWithRotation(rotation);
            return material;
        }
    }
}
