using System.ComponentModel;
using System.Text.Json.Serialization;
using Tavenem.DataStorage;

namespace Tavenem.Chemistry;

/// <summary>
/// One or more chemicals as a homogeneous or heterogeneous mixture, or as a physical composite
/// of unmixed components.
/// </summary>
[TypeConverter(typeof(SubstanceConverter))]
[JsonConverter(typeof(ISubstanceConverter))]
public interface ISubstance : IIdItem, IEquatable<ISubstance>, IEquatable<ISubstanceReference>
{
    /// <summary>
    /// <para>
    /// The collection of constituents that make up this substance, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </para>
    /// <para>
    /// In a homogeneous substance, may contain only a reference to the instance itself, along
    /// with a proportion of 1.
    /// </para>
    /// </summary>
    IReadOnlyDictionary<HomogeneousReference, decimal> Constituents { get; }

    /// <summary>
    /// The approximate density of this substance in the liquid phase, in kg/m³.
    /// </summary>
    /// <remarks>
    /// Density varies with pressure and temperature, but not by much in the liquid phase.
    /// </remarks>
    double? DensityLiquid { get; }

    /// <summary>
    /// The approximate density of this substance in the solid phase, in kg/m³.
    /// </summary>
    /// <remarks>
    /// Density varies with pressure and temperature, but not by much in the solid phase.
    /// </remarks>
    double? DensitySolid { get; }

    /// <summary>
    /// The approximate density of this substance when its phase is neither solid, liquid, nor
    /// gas, in kg/m³.
    /// </summary>
    /// <remarks>
    /// For instance, a substance in the glass phase may have a special density.
    /// </remarks>
    double? DensitySpecial { get; }

    /// <summary>
    /// Indicates the average greenhouse potential (a.k.a. global warming potential, GWP) of
    /// this substance compared to CO₂, over 100 years.
    /// </summary>
    double GreenhousePotential { get; }

    /// <summary>
    /// The hardness of this substance as a solid, in MPa.
    /// </summary>
    /// <remarks>
    /// Measurements may be from any applicable scale, but should be converted to standardized
    /// MPa. It is recognized that discrepancies between measurement scales exist which prevent
    /// consistent standardization to any unit, but these factors are disregarded in favor of a
    /// single unit with the broadest scope possible.
    /// </remarks>
    double Hardness { get; }

    /// <summary>
    /// Indicates whether this substance contains no constituents.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Indicates whether this substance conducts electricity.
    /// </summary>
    bool IsConductive { get; }

    /// <summary>
    /// Indicates whether this substance is able to burn.
    /// </summary>
    bool IsFlammable { get; }

    /// <summary>
    /// Indicates whether this substance is considered a gemstone.
    /// </summary>
    bool IsGemstone { get; }

    /// <summary>
    /// <para>
    /// Indicates whether this substance is a metal.
    /// </para>
    /// <para>
    /// When not set explicitly, this is indicated by the inclusion in its chemical formula of at
    /// least as many metallic elements as non-metallic, not counting metalloids.
    /// </para>
    /// </summary>
    bool IsMetal { get; }

    /// <summary>
    /// Indicates whether this substance is radioactive.
    /// </summary>
    bool IsRadioactive { get; }

    /// <summary>
    /// The molar mass of this substance, in kg/mol.
    /// </summary>
    double MolarMass { get; }

    /// <summary>
    /// A name for this substance.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// <para>
    /// The Young's modulus of this substance, in GPa.
    /// </para>
    /// <para>
    /// May be <see langword="null"/>, which indicates no known value.
    /// </para>
    /// </summary>
    double? YoungsModulus { get; }

    /// <summary>
    /// <para>
    /// Adds the given <paramref name="constituent"/> to this instance at the given <paramref
    /// name="proportion"/>. The exact behavior depends on the type of this instance, but the
    /// result will always be a new <see cref="ISubstance"/> instance.
    /// </para>
    /// <para>
    /// Or, if proportion is greater than or equal to 1, returns the given <paramref
    /// name="constituent"/>.
    /// </para>
    /// </summary>
    /// <param name="constituent">A homogeneous substance to add as a constituent of this
    /// substance. If this substance already contains the constituent, its proportion is
    /// adjusted to the new value.</param>
    /// <param name="proportion">
    /// <para>
    /// The proportion at which to add the given <paramref name="constituent"/>.
    /// </para>
    /// <para>
    /// The proportions of the other constituents of this substance will be reduced
    /// proportionately to accomodate this value.
    /// </para>
    /// <para>
    /// If less than or equal to zero, this instance is returned unchanged.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="constituent"/>, or if <paramref
    /// name="proportion"/>
    /// is greater than or equal to 1, the given <paramref name="constituent"/>.</returns>
    ISubstance AddConstituent(HomogeneousReference constituent, decimal proportion = 0.5m);

    /// <summary>
    /// <para>
    /// Adds the given <paramref name="constituent"/> to this instance at the given <paramref
    /// name="proportion"/>. The exact behavior depends on the type of this instance, but the
    /// result will always be a new <see cref="ISubstance"/> instance.
    /// </para>
    /// <para>
    /// Or, if proportion is greater than or equal to 1, returns the given <paramref
    /// name="constituent"/>.
    /// </para>
    /// </summary>
    /// <param name="constituent">A homogeneous substance to add as a constituent of this
    /// substance. If this substance already contains the constituent, its proportion is
    /// adjusted to the new value.</param>
    /// <param name="proportion">
    /// <para>
    /// The proportion at which to add the given <paramref name="constituent"/>.
    /// </para>
    /// <para>
    /// The proportions of the other constituents of this substance will be reduced
    /// proportionately to accomodate this value.
    /// </para>
    /// <para>
    /// If less than or equal to zero, this instance is returned unchanged.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="constituent"/>, or if <paramref
    /// name="proportion"/>
    /// is greater than or equal to 1, the given <paramref name="constituent"/>.</returns>
    ISubstance AddConstituent(IHomogeneous constituent, decimal proportion = 0.5m);

    /// <summary>
    /// Combines the given <paramref name="substance"/> with this instance. The exact behavior
    /// depends on the type of both this instance and the <paramref name="substance"/> added,
    /// but the result will always be a new <see cref="ISubstance"/> instance.
    /// </summary>
    /// <param name="substance">An <see cref="ISubstance"/> instance to combine with this
    /// one.</param>
    /// <param name="proportion">
    /// <para>
    /// The proportion at which to add the given <paramref name="substance"/>.
    /// </para>
    /// <para>
    /// The proportions of the individual constituents of each substance will be reduced
    /// proportionately to accomodate this value.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="substance"/>.</returns>
    ISubstance Combine(ISubstanceReference substance, decimal proportion = 0.5m);

    /// <summary>
    /// Combines the given <paramref name="substance"/> with this instance. The exact behavior
    /// depends on the type of both this instance and the <paramref name="substance"/> added,
    /// but the result will always be a new <see cref="ISubstance"/> instance.
    /// </summary>
    /// <param name="substance">An <see cref="ISubstance"/> instance to combine with this
    /// one.</param>
    /// <param name="proportion">
    /// <para>
    /// The proportion at which to add the given <paramref name="substance"/>.
    /// </para>
    /// <para>
    /// The proportions of the individual constituents of each substance will be reduced
    /// proportionately to accomodate this value.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="substance"/>.</returns>
    ISubstance Combine(ISubstance substance, decimal proportion = 0.5m);

    /// <summary>
    /// Determines whether this substance contains the given constituent in the given phase.
    /// </summary>
    /// <param name="substance">A substance to test.</param>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <param name="phase">The phase to test.</param>
    /// <returns><see langword="true"/> if the given <paramref name="substance"/> is present in
    /// the given <paramref name="phase"/>; otherwise <see langword="false"/>.</returns>
    bool Contains(HomogeneousReference substance, double temperature, double pressure, PhaseType phase = PhaseType.Any);

    /// <summary>
    /// Determines whether this substance contains the given constituent in the given phase.
    /// </summary>
    /// <param name="substance">A substance to test.</param>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <param name="phase">The phase to test.</param>
    /// <returns><see langword="true"/> if the given <paramref name="substance"/> is present in
    /// the given <paramref name="phase"/>; otherwise <see langword="false"/>.</returns>
    bool Contains(IHomogeneous substance, double temperature, double pressure, PhaseType phase = PhaseType.Any);

    /// <summary>
    /// <para>
    /// Enumerates the chemical constituents of this substance.
    /// </para>
    /// <para>
    /// Note that some constituents of a substance may be non-chemical components. Such
    /// constituents will be omitted from this enumeration.
    /// </para>
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Chemical"/>
    /// constituents.</returns>
    IEnumerable<Chemical> GetChemicalConstituents();

    /// <summary>
    /// Gets the approximate average density of this substance under the given conditions of
    /// <paramref name="temperature"/> and <paramref name="pressure"/>, in kg/m³.
    /// </summary>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <returns>The approximate average density of this substance under the given conditions of
    /// <paramref name="temperature"/> and <paramref name="pressure"/>, in kg/m³.</returns>
    double GetDensity(double temperature, double pressure);

    /// <summary>
    /// <para>
    /// In heterogeneous composites, gets a homogenized version of the mixture.
    /// </para>
    /// <para>
    /// In other substances, gets the substance itself.
    /// </para>
    /// </summary>
    /// <returns>A homogenized version of a heterogeneous composites, or the substance
    /// itself.</returns>
    ISubstance GetHomogenized();

    /// <summary>
    /// Gets the proportion of the given <paramref name="constituent"/> in this substance.
    /// </summary>
    /// <param name="constituent">An <see cref="IHomogeneous"/> constituent whose proportion in
    /// this instance will be determined.</param>
    /// <returns>The proportion of the given <paramref name="constituent"/> in this substance;
    /// or zero, if it does not contain the given <paramref name="constituent"/>.</returns>
    decimal GetProportion(HomogeneousReference constituent)
    {
        if (constituent.Equals(this))
        {
            return 1;
        }
        else if (Constituents is null)
        {
            return 0;
        }
        else if (Constituents.TryGetValue(constituent, out var value))
        {
            return value;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets the proportion of the given <paramref name="constituent"/> in this substance.
    /// </summary>
    /// <param name="constituent">An <see cref="IHomogeneous"/> constituent whose proportion in
    /// this instance will be determined.</param>
    /// <returns>The proportion of the given <paramref name="constituent"/> in this substance;
    /// or zero, if it does not contain the given <paramref name="constituent"/>.</returns>
    decimal GetProportion(IHomogeneous constituent)
    {
        if (constituent.Equals(this))
        {
            return 1;
        }
        else if (Constituents is null)
        {
            return 0;
        }
        else if (Constituents.TryGetValue(constituent.GetHomogeneousReference(), out var value))
        {
            return value;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Get the proportion of constituents of this substance which satisfy the given
    /// <paramref name="condition"/>.
    /// </summary>
    /// <param name="condition">A <see cref="Predicate{T}"/> of <see cref="IHomogeneous"/> to
    /// test each constituent of this substance.</param>
    /// <returns>The overall proportion of constituents of this substance which satisfy the
    /// given <paramref name="condition"/>, as a value between 0 and 1.</returns>
    decimal GetProportion(Predicate<IHomogeneous> condition)
        => this is IHomogeneous h && condition.Invoke(h)
        ? 1
        : Constituents?.Sum(x => condition.Invoke(x.Key.Homogeneous) ? x.Value : 0) ?? 0;

    /// <summary>
    /// Gets an <see cref="ISubstanceReference"/> for this <see cref="ISubstance"/>.
    /// </summary>
    /// <returns>An <see cref="ISubstanceReference"/> for this <see
    /// cref="ISubstance"/>.</returns>
    ISubstanceReference GetReference();

    /// <summary>
    /// Gets a new substance without the given <paramref name="constituent"/>.
    /// </summary>
    /// <param name="constituent">A substance to remove.</param>
    /// <returns>A new substance without the given <paramref name="constituent"/>; may be
    /// empty.</returns>
    ISubstance Remove(HomogeneousReference constituent);

    /// <summary>
    /// Gets a new substance without the given <paramref name="constituent"/>.
    /// </summary>
    /// <param name="constituent">A substance to remove.</param>
    /// <returns>A new substance without the given <paramref name="constituent"/>; may be
    /// empty.</returns>
    ISubstance Remove(IHomogeneous constituent);

    /// <summary>
    /// Separates this substance into components by phase.
    /// </summary>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <param name="phases">One or more phase(s) to separate.</param>
    /// <returns>
    /// <para>
    /// An enumeration of tuples, each containing a list of substances which represents the
    /// fraction of this instance's components which are in the given phase(s) of that position
    /// in the <paramref name="phases"/> params array under the given conditions of <paramref
    /// name="temperature"/> and <paramref name="pressure"/>, along with the proportion of the
    /// whole represented by that fraction. The final value in the enumeration will contain all
    /// those constituents which did not match any phase(s) given.
    /// </para>
    /// <para>
    /// Note that the proportions of the results may sum to a value greater than 1 if the phases
    /// provided overlap, such that a constituent of this instance satisfies more than one
    /// criteria.
    /// </para>
    /// </returns>
    IEnumerable<(List<ISubstanceReference> components, decimal proportion)> SeparateByPhase(double temperature, double pressure, params PhaseType[] phases);

    /// <summary>
    /// Gets a copy of this instance with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">A new name for this instance.</param>
    /// <returns>A version of this instance with the given name.</returns>
    ISubstance WithSubstanceName(string name);
}
