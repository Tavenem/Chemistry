using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Tavenem.DataStorage;

namespace Tavenem.Chemistry;

/// <summary>
/// Describes the properties of a homogenous substance whose chemical constituents are either
/// unknown, or undescribed for any reason.
/// </summary>
/// <remarks>
/// This structure is intended to facilitate convenient representation of complex composite
/// materials, without requiring their base-level molecular formulae to be precisely enumerated.
/// For instance, macromolecular biological structures or complex industrial compositions, whose
/// aggregate properties are known but whose constituents are either unknown, are variable in
/// specific samples, or are too intricate to model conveniently.
/// </remarks>
[TypeConverter(typeof(SubstanceConverter))]
public class HomogeneousSubstance : IHomogeneous, IEquatable<HomogeneousSubstance>
{
    /// <summary>
    /// A value used to generate a unique key for this <see cref="ISubstance"/>.
    /// </summary>
    public const ushort TypeKey = 1;

    private const string NoneName = "None";
    /// <summary>
    /// An empty <see cref="HomogeneousSubstance"/> instance.
    /// </summary>
    public static readonly HomogeneousSubstance None = new(NoneName);

    /// <summary>
    /// The "A" Antoine coefficient which can be used to determine the vapor pressure of this substance.
    /// </summary>
    public double? AntoineCoefficientA { get; }

    /// <summary>
    /// The "B" Antoine coefficient which can be used to determine the vapor pressure of this substance.
    /// </summary>
    public double? AntoineCoefficientB { get; }

    /// <summary>
    /// The "C" Antoine coefficient which can be used to determine the vapor pressure of this substance.
    /// </summary>
    public double? AntoineCoefficientC { get; }

    /// <summary>
    /// The upper limit of the Antoine coefficients' accuracy for this substance. It is presumed
    /// reasonable to assume that the substance always vaporizes above this temperature.
    /// </summary>
    public double? AntoineMaximumTemperature { get; }

    /// <summary>
    /// The lower limit of the Antoine coefficients' accuracy for this substance. It is presumed
    /// reasonable to assume that the substance always condenses below this temperature.
    /// </summary>
    public double? AntoineMinimumTemperature { get; }

    /// <summary>
    /// An optional list of common names for this substance.
    /// </summary>
    /// <remarks>
    /// The list may be arranged in order of most to least common, so that the first name in the
    /// list (if a list is present at all) can be assumed to be the most recognizable name for the
    /// substance. However, this is not a strict requirement. Names may appear in any order,
    /// particularly if no specific usage data is available, or when various names are equally
    /// common in different contexts.
    /// </remarks>
    public IReadOnlyList<string>? CommonNames { get; }

    /// <summary>
    /// <para>
    /// The collection of constituents that make up this substance, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </para>
    /// <para>
    /// Always contains only this instance itself, with a proportion of 1.
    /// </para>
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<HomogeneousReference, decimal> Constituents
        => new ReadOnlyDictionary<HomogeneousReference, decimal>(new Dictionary<HomogeneousReference, decimal> { { GetHomogeneousReference(), 1 } });

    /// <summary>
    /// The approximate density of this substance in the liquid phase, in kg/m³.
    /// </summary>
    /// <remarks>
    /// Density varies with pressure and temperature, but not by much in the liquid phase.
    /// </remarks>
    public double? DensityLiquid { get; }

    /// <summary>
    /// The approximate density of this substance in the solid phase, in kg/m³.
    /// </summary>
    /// <remarks>
    /// Density varies with pressure and temperature, but not by much in the solid phase.
    /// </remarks>
    public double? DensitySolid { get; }

    /// <summary>
    /// The approximate density of this substance when its phase is neither solid, liquid, nor
    /// gas, in kg/m³.
    /// </summary>
    /// <remarks>
    /// For instance, a substance in the glass phase may have a special density.
    /// </remarks>
    public double? DensitySpecial { get; }

    /// <summary>
    /// If set, indicates an explicitly defined phase for this substance, which overrides the
    /// usual phase calculations based on temperature and pressure.
    /// </summary>
    /// <remarks>
    /// This is expected to be utilized mainly for substances in exotic phases of matter, such
    /// as plasma, glass, etc. These phases are not indicated using the standard <see
    /// cref="IHomogeneous.GetPhase(double, double)"/> method.
    /// </remarks>
    public PhaseType? FixedPhase { get; }

    /// <summary>
    /// Indicates the average greenhouse potential (a.k.a. global warming potential, GWP) of
    /// this substance compared to CO₂, over 100 years.
    /// </summary>
    public double GreenhousePotential { get; }

    /// <summary>
    /// The hardness of this substance as a solid, in MPa.
    /// </summary>
    /// <remarks>
    /// Measurements may be from any applicable scale, but should be converted to standardized
    /// MPa. It is recognized that discrepancies between measurement scales exist which prevent
    /// consistent standardization to any unit, but these factors are disregarded in favor of a
    /// single unit with the broadest scope possible.
    /// </remarks>
    public double Hardness { get; }

    /// <summary>
    /// The ID of this item.
    /// </summary>
    [JsonPropertyName("id"), JsonPropertyOrder(-1)]
    public string Id { get; }

    /// <summary>
    /// The <see cref="IIdItem.IdItemTypeName"/> for <see cref="HomogeneousSubstance"/>.
    /// </summary>
    public const string HomogeneousSubstanceIdItemTypeName = ":HomogeneousSubstance:";
    /// <summary>
    /// A built-in, read-only type discriminator.
    /// </summary>
    [JsonPropertyName("$type"), JsonPropertyOrder(-2)]
    public string IdItemTypeName => HomogeneousSubstanceIdItemTypeName;

    /// <summary>
    /// Indicates whether this substance conducts electricity.
    /// </summary>
    public bool IsConductive { get; }

    /// <summary>
    /// Indicates whether this instance is the same as <see cref="None"/>.
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Equals(None);

    /// <summary>
    /// Indicates whether this substance is able to burn.
    /// </summary>
    public bool IsFlammable { get; }

    /// <summary>
    /// Indicates whether this substance is considered a gemstone.
    /// </summary>
    public bool IsGemstone { get; }

    /// <summary>
    /// <para>
    /// Indicates whether this substance is a metal.
    /// </para>
    /// <para>
    /// When not set explicitly, this is indicated by the inclusion in its chemical formula of at
    /// least as many metallic elements as non-metallic, not counting metalloids.
    /// </para>
    /// </summary>
    public bool IsMetal { get; }

    /// <summary>
    /// <para>
    /// Indicates whether this substance is radioactive.
    /// </para>
    /// <para>
    /// When not set explicitly, this is indicated by the inclusion in its chemical formula of any
    /// radioactive isotopes.
    /// </para>
    /// </summary>
    public bool IsRadioactive { get; }

    /// <summary>
    /// The melting point of this substance at 100 kPa, in K.
    /// </summary>
    public double? MeltingPoint { get; }

    /// <summary>
    /// The molar mass of this substance, in kg/mol.
    /// </summary>
    public double MolarMass { get; }

    /// <summary>
    /// The name of this substance.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// <para>
    /// The Young's modulus of this substance, in GPa.
    /// </para>
    /// <para>
    /// May be <see langword="null"/>, which indicates no known value.
    /// </para>
    /// </summary>
    public double? YoungsModulus { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="HomogeneousSubstance"/>.
    /// </summary>
    /// <param name="name">The name of this substance.</param>
    /// <param name="antoineCoefficientA">
    /// <para>
    /// The "A" Antoine coefficient.
    /// </para>
    /// <para>
    /// Whether the phase can be indicated as gaseous depends on whether the Antoine
    /// coefficients, minimum and/or maximum temperatures have been defined.
    /// </para>
    /// <para>
    /// When the Antoine coefficients are unknown, but a boiling point at STP is known, the
    /// minimum and maximum may both be set to this temperature. This produces inaccurate
    /// results at non-standard pressures, of course, but permits at least some separation
    /// between the liquid and gas phases when a precise formula cannot be determined.
    /// </para>
    /// </param>
    /// <param name="antoineCoefficientB">
    /// The "B" Antoine coefficient.
    /// </param>
    /// <param name="antoineCoefficientC">
    /// The "C" Antoine coefficient.
    /// </param>
    /// <param name="antoineMaximumTemperature">
    /// <para>
    /// A maximum Antoine temperature, in K.
    /// </para>
    /// <para>
    /// A maximum temperature of <see cref="double.NegativeInfinity"/> may be defined to
    /// indicate that a chemical is always gaseous. There is no need to specify the Antoine
    /// coefficients in this case.
    /// </para>
    /// <para>
    /// When the Antoine coefficients are unknown, but a boiling point at STP is known, the
    /// minimum and maximum may both be set to this temperature. This produces inaccurate
    /// results at non-standard pressures, of course, but permits at least some separation
    /// between the liquid and gas phases when a precise formula cannot be determined.
    /// </para>
    /// </param>
    /// <param name="antoineMinimumTemperature">
    /// <para>
    /// A minimum Antoine temperature, in K.
    /// </para>
    /// <para>
    /// A minimum temperature of <see cref="double.PositiveInfinity"/> may be defined to
    /// indicate that a chemical is never gaseous. There is no need to specify the Antoine
    /// coefficients in this case.
    /// </para>
    /// <para>
    /// When the Antoine coefficients are unknown, but a boiling point at STP is known, the
    /// minimum and maximum may both be set to this temperature. This produces inaccurate
    /// results at non-standard pressures, of course, but permits at least some separation
    /// between the liquid and gas phases when a precise formula cannot be determined.
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the substance in the liquid phase,
    /// in kg/m³.</param>
    /// <param name="densitySolid">The approximate density of the substance in the solid phase,
    /// in kg/m³.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="greenhousePotential">A greenhouse potential.</param>
    /// <param name="hardness">The hardness of the substance as a solid, in MPa.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the substance is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the substance is flammable.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="isMetal">Whether or not the substance is a metal.</param>
    /// <param name="isRadioactive">Indicates whether this substance is radioactive.</param>
    /// <param name="meltingPoint">
    /// <para>
    /// A melting point, in K.
    /// </para>
    /// <para>
    /// If the melting point of the substance is not indicated, the phase will never be indicated
    /// as liquid. A melting point of <see cref="double.NegativeInfinity"/> may be given to
    /// indicate that a substance is always liquid.
    /// </para>
    /// </param>
    /// <param name="molarMass">The molar mass of this substance, in kg/mol.</param>
    /// <param name="fixedPhase">
    /// <para>
    /// If set, indicates an explicitly defined phase for this substance, which overrides the
    /// usual phase calculations based on temperature and pressure.
    /// </para>
    /// <para>
    /// This is expected to be utilized mainly for substances in exotic phases of matter, such
    /// as plasma, glass, etc. These phases are not indicated using the standard <see
    /// cref="IHomogeneous.GetPhase(double, double)"/> method.
    /// </para>
    /// </param>
    /// <param name="youngsModulus">
    /// <para>
    /// The Young's Modulus of this chemical, in GPa.
    /// </para>
    /// <para>May be left <see langword="null"/> to indicate no known value.
    /// </para>
    /// </param>
    /// <param name="commonNames">
    /// <para>
    /// An optional list of common names for this substance.
    /// </para>
    /// <para>
    /// The list may be arranged in order of most to least common, so that the first name in the
    /// list (if a list is present at all) can be assumed to be the most recognizable name for the
    /// substance. However, this is not a strict requirement. Names may appear in any order,
    /// particularly if no specific usage data is available, or when various names are equally
    /// common in different contexts.
    /// </para>
    /// </param>
    public HomogeneousSubstance(
        string name,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? greenhousePotential = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool? isGemstone = null,
        bool? isMetal = null,
        bool? isRadioactive = null,
        double? meltingPoint = null,
        double? molarMass = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null,
        params string[] commonNames) : this(
            Guid.NewGuid().ToString(),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            greenhousePotential ?? 0,
            hardness ?? 0,
            isConductive ?? isMetal ?? false,
            isFlammable ?? false,
            isGemstone ?? false,
            isMetal ?? false,
            isRadioactive ?? false,
            meltingPoint,
            molarMass ?? 0,
            fixedPhase,
            youngsModulus,
            commonNames)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="HomogeneousSubstance"/>.
    /// </summary>
    /// <param name="id">The unique ID of this substance.</param>
    /// <param name="name">The name of this substance.</param>
    /// <param name="antoineCoefficientA">
    /// <para>
    /// The "A" Antoine coefficient.
    /// </para>
    /// <para>
    /// Whether the phase can be indicated as gaseous depends on whether the Antoine
    /// coefficients, minimum and/or maximum temperatures have been defined.
    /// </para>
    /// <para>
    /// When the Antoine coefficients are unknown, but a boiling point at STP is known, the
    /// minimum and maximum may both be set to this temperature. This produces inaccurate
    /// results at non-standard pressures, of course, but permits at least some separation
    /// between the liquid and gas phases when a precise formula cannot be determined.
    /// </para>
    /// </param>
    /// <param name="antoineCoefficientB">
    /// The "B" Antoine coefficient.
    /// </param>
    /// <param name="antoineCoefficientC">
    /// The "C" Antoine coefficient.
    /// </param>
    /// <param name="antoineMaximumTemperature">
    /// <para>
    /// A maximum Antoine temperature, in K.
    /// </para>
    /// <para>
    /// A maximum temperature of <see cref="double.NegativeInfinity"/> may be defined to
    /// indicate that a chemical is always gaseous. There is no need to specify the Antoine
    /// coefficients in this case.
    /// </para>
    /// <para>
    /// When the Antoine coefficients are unknown, but a boiling point at STP is known, the
    /// minimum and maximum may both be set to this temperature. This produces inaccurate
    /// results at non-standard pressures, of course, but permits at least some separation
    /// between the liquid and gas phases when a precise formula cannot be determined.
    /// </para>
    /// </param>
    /// <param name="antoineMinimumTemperature">
    /// <para>
    /// A minimum Antoine temperature, in K.
    /// </para>
    /// <para>
    /// A minimum temperature of <see cref="double.PositiveInfinity"/> may be defined to
    /// indicate that a chemical is never gaseous. There is no need to specify the Antoine
    /// coefficients in this case.
    /// </para>
    /// <para>
    /// When the Antoine coefficients are unknown, but a boiling point at STP is known, the
    /// minimum and maximum may both be set to this temperature. This produces inaccurate
    /// results at non-standard pressures, of course, but permits at least some separation
    /// between the liquid and gas phases when a precise formula cannot be determined.
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the substance in the liquid phase,
    /// in kg/m³.</param>
    /// <param name="densitySolid">The approximate density of the substance in the solid phase,
    /// in kg/m³.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="greenhousePotential">A greenhouse potential.</param>
    /// <param name="hardness">The hardness of the substance as a solid, in MPa.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the substance is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the substance is flammable.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="isMetal">Whether or not the substance is a metal.</param>
    /// <param name="isRadioactive">Indicates whether this substance is radioactive.</param>
    /// <param name="meltingPoint">
    /// <para>
    /// A melting point, in K.
    /// </para>
    /// <para>
    /// If the melting point of the substance is not indicated, the phase will never be indicated
    /// as liquid. A melting point of <see cref="double.NegativeInfinity"/> may be given to
    /// indicate that a substance is always liquid.
    /// </para>
    /// </param>
    /// <param name="molarMass">The molar mass of this substance, in kg/mol.</param>
    /// <param name="fixedPhase">
    /// <para>
    /// If set, indicates an explicitly defined phase for this substance, which overrides the
    /// usual phase calculations based on temperature and pressure.
    /// </para>
    /// <para>
    /// This is expected to be utilized mainly for substances in exotic phases of matter, such
    /// as plasma, glass, etc. These phases are not indicated using the standard <see
    /// cref="IHomogeneous.GetPhase(double, double)"/> method.
    /// </para>
    /// </param>
    /// <param name="youngsModulus">
    /// <para>
    /// The Young's Modulus of this chemical, in GPa.
    /// </para>
    /// <para>May be left <see langword="null"/> to indicate no known value.
    /// </para>
    /// </param>
    /// <param name="commonNames">
    /// <para>
    /// An optional list of common names for this substance.
    /// </para>
    /// <para>
    /// The list may be arranged in order of most to least common, so that the first name in the
    /// list (if a list is present at all) can be assumed to be the most recognizable name for the
    /// substance. However, this is not a strict requirement. Names may appear in any order,
    /// particularly if no specific usage data is available, or when various names are equally
    /// common in different contexts.
    /// </para>
    /// </param>
    [JsonConstructor]
    public HomogeneousSubstance(
        string id,
        string name,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double greenhousePotential = 0,
        double hardness = 0,
        bool isConductive = false,
        bool isFlammable = false,
        bool isGemstone = false,
        bool isMetal = false,
        bool isRadioactive = false,
        double? meltingPoint = null,
        double molarMass = 0,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null,
        IReadOnlyList<string>? commonNames = null)
    {
        Id = id;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        Name = name;
        AntoineCoefficientA = antoineCoefficientA.HasValue && antoineCoefficientB.HasValue && antoineCoefficientC.HasValue
            ? antoineCoefficientA.Value
            : (double?)null;
        AntoineCoefficientB = antoineCoefficientA.HasValue && antoineCoefficientB.HasValue && antoineCoefficientC.HasValue
            ? antoineCoefficientB.Value
            : (double?)null;
        AntoineCoefficientC = antoineCoefficientA.HasValue && antoineCoefficientB.HasValue && antoineCoefficientC.HasValue
            ? antoineCoefficientC.Value
            : (double?)null;
        AntoineMaximumTemperature = antoineMaximumTemperature;
        AntoineMinimumTemperature = antoineMinimumTemperature;
        DensityLiquid = densityLiquid;
        DensitySolid = densitySolid;
        DensitySpecial = densitySpecial;
        GreenhousePotential = greenhousePotential;
        Hardness = hardness;
        IsFlammable = isFlammable;
        IsGemstone = isGemstone;
        IsMetal = isMetal;
        IsRadioactive = isRadioactive;
        MeltingPoint = meltingPoint;
        IsConductive = isConductive;
        MolarMass = molarMass;
        FixedPhase = fixedPhase;
        YoungsModulus = youngsModulus;
        CommonNames = commonNames;
    }

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
    /// proportionately to accommodate this value.
    /// </para>
    /// <para>
    /// If less than or equal to zero, this instance is returned unchanged.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="constituent"/>, or if <paramref
    /// name="proportion"/>
    /// is greater than or equal to 1, the given <paramref name="constituent"/>.</returns>
    public ISubstance AddConstituent(HomogeneousReference constituent, decimal proportion = 0.5m)
    {
        if (proportion >= 1)
        {
            return constituent.Homogeneous;
        }
        if (proportion <= 0)
        {
            return this;
        }

        return new Mixture(null, null, null, null, (GetHomogeneousReference(), 1 - proportion), (constituent, proportion));
    }

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
    /// proportionately to accommodate this value.
    /// </para>
    /// <para>
    /// If less than or equal to zero, this instance is returned unchanged.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="constituent"/>, or if <paramref
    /// name="proportion"/>
    /// is greater than or equal to 1, the given <paramref name="constituent"/>.</returns>
    public ISubstance AddConstituent(IHomogeneous constituent, decimal proportion = 0.5m)
    {
        if (proportion >= 1)
        {
            return constituent;
        }
        if (proportion <= 0)
        {
            return this;
        }

        return new Mixture(null, null, null, null, (this, 1 - proportion), (constituent, proportion));
    }

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
    /// proportionately to accommodate this value.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="substance"/>.</returns>
    public ISubstance Combine(ISubstanceReference substance, decimal proportion = 0.5m)
    {
        if (proportion >= 1)
        {
            return substance.Substance;
        }
        if (proportion <= 0)
        {
            return this;
        }

        var s = substance.Substance;
        if (s is IHomogeneous homogeneous)
        {
            return new Mixture(null, null, null, null, (this, 1 - proportion), (homogeneous, proportion));
        }
        return s.Combine(this, 1 - proportion);
    }

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
    /// proportionately to accommodate this value.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="substance"/>.</returns>
    public ISubstance Combine(ISubstance substance, decimal proportion = 0.5m)
    {
        if (proportion >= 1)
        {
            return substance;
        }
        if (proportion <= 0)
        {
            return this;
        }

        if (substance is IHomogeneous homogeneous)
        {
            return new Mixture(null, null, null, null, (this, 1 - proportion), (homogeneous, proportion));
        }
        return substance.Combine(this, 1 - proportion);
    }

    /// <summary>
    /// Determines whether this substance contains the given constituent in the given phase.
    /// </summary>
    /// <param name="substance">A substance to test.</param>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <param name="phase">The phase to test.</param>
    /// <returns><see langword="true"/> if the given <paramref name="substance"/> is present in
    /// the given <paramref name="phase"/>; otherwise <see langword="false"/>.</returns>
    public bool Contains(HomogeneousReference substance, double temperature, double pressure, PhaseType phase = PhaseType.Any)
        => Constituents.Any(x => x.Key.Equals(substance) && (phase == PhaseType.Any || (x.Key.Homogeneous.GetPhase(temperature, pressure) & phase) != PhaseType.None));

    /// <summary>
    /// Determines whether this substance contains the given constituent in the given phase.
    /// </summary>
    /// <param name="substance">A substance to test.</param>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <param name="phase">The phase to test.</param>
    /// <returns><see langword="true"/> if the given <paramref name="substance"/> is present in
    /// the given <paramref name="phase"/>; otherwise <see langword="false"/>.</returns>
    public bool Contains(IHomogeneous substance, double temperature, double pressure, PhaseType phase = PhaseType.Any)
        => Constituents.Any(x => x.Key.Equals(substance) && (phase == PhaseType.Any || (x.Key.Homogeneous.GetPhase(temperature, pressure) & phase) != PhaseType.None));

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(HomogeneousSubstance? other)
        => other is not null && Id.Equals(other.Id, StringComparison.Ordinal);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(IIdItem? other)
        => other is HomogeneousSubstance substance && Equals(substance);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ISubstance? other)
        => other is HomogeneousSubstance substance && Equals(substance);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(HomogeneousReference? other)
        => other?.Equals(this) == true;

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ISubstanceReference? other)
        => other is HomogeneousReference reference && reference.Equals(this);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance
    /// are the same type and represent the same value; otherwise, <see
    /// langword="false"/>.</returns>
    public override bool Equals(object? obj)
        => (obj is ISubstance substance && Equals(substance))
        || (obj is ISubstanceReference reference && Equals(reference));

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
    public IEnumerable<Chemical> GetChemicalConstituents()
    {
        yield break;
    }

    /// <summary>
    /// Gets the approximate average density of this substance under the given conditions of
    /// <paramref name="temperature"/> and <paramref name="pressure"/>, in kg/m³.
    /// </summary>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <returns>The approximate average density of this chemical under the given conditions of
    /// <paramref name="temperature"/> and <paramref name="pressure"/>, in kg/m³.</returns>
    public double GetDensity(double temperature, double pressure)
    {
        var phase = GetPhase(temperature, pressure);
        if (phase == PhaseType.Solid && DensitySolid.HasValue)
        {
            return DensitySolid.Value;
        }
        if (phase == PhaseType.Liquid && DensityLiquid.HasValue)
        {
            return DensityLiquid.Value;
        }
        if (DensitySpecial.HasValue
            && phase != PhaseType.Solid
            && phase != PhaseType.Liquid
            && phase != PhaseType.Gas)
        {
            return DensitySpecial.Value;
        }
        return pressure * MolarMass / (Mathematics.DoubleConstants.UniversalGasConstant * temperature);
    }

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
        var hashCode = -1641914381;
        hashCode = (hashCode * -1521134295) + (AntoineCoefficientA.HasValue ? EqualityComparer<double>.Default.GetHashCode(AntoineCoefficientA.Value) : 0);
        hashCode = (hashCode * -1521134295) + (AntoineCoefficientB.HasValue ? EqualityComparer<double>.Default.GetHashCode(AntoineCoefficientB.Value) : 0);
        hashCode = (hashCode * -1521134295) + (AntoineCoefficientC.HasValue ? EqualityComparer<double>.Default.GetHashCode(AntoineCoefficientC.Value) : 0);
        hashCode = (hashCode * -1521134295) + (AntoineMaximumTemperature.HasValue ? EqualityComparer<double>.Default.GetHashCode(AntoineMaximumTemperature.Value) : 0);
        hashCode = (hashCode * -1521134295) + (AntoineMinimumTemperature.HasValue ? EqualityComparer<double>.Default.GetHashCode(AntoineMinimumTemperature.Value) : 0);
        hashCode = (hashCode * -1521134295) + (DensityLiquid.HasValue ? EqualityComparer<double>.Default.GetHashCode(DensityLiquid.Value) : 0);
        hashCode = (hashCode * -1521134295) + (DensitySolid.HasValue ? EqualityComparer<double>.Default.GetHashCode(DensitySolid.Value) : 0);
        hashCode = (hashCode * -1521134295) + (FixedPhase.HasValue ? EqualityComparer<PhaseType>.Default.GetHashCode(FixedPhase.Value) : 0);
        hashCode = (hashCode * -1521134295) + GreenhousePotential.GetHashCode();
        hashCode = (hashCode * -1521134295) + Hardness.GetHashCode();
        hashCode = (hashCode * -1521134295) + IsConductive.GetHashCode();
        hashCode = (hashCode * -1521134295) + IsFlammable.GetHashCode();
        hashCode = (hashCode * -1521134295) + IsMetal.GetHashCode();
        hashCode = (hashCode * -1521134295) + IsRadioactive.GetHashCode();
        hashCode = (hashCode * -1521134295) + (MeltingPoint.HasValue ? EqualityComparer<double>.Default.GetHashCode(MeltingPoint.Value) : 0);
        hashCode = (hashCode * -1521134295) + MolarMass.GetHashCode();
        return (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name);
    }

    /// <summary>
    /// Gets an <see cref="HomogeneousReference"/> for this <see cref="HomogeneousSubstance"/>.
    /// </summary>
    /// <returns>An <see cref="HomogeneousReference"/> for this <see
    /// cref="HomogeneousSubstance"/>.</returns>
    public HomogeneousReference GetHomogeneousReference() => new(this);

    /// <summary>
    /// <para>
    /// In heterogeneous composites, gets a homogenized version of the mixture.
    /// </para>
    /// <para>
    /// In a <see cref="HomogeneousSubstance"/>, gets the instance, unchanged.
    /// </para>
    /// </summary>
    /// <returns>A homogenized version of a heterogeneous composites, or the substance
    /// itself.</returns>
    public ISubstance GetHomogenized() => this;

    /// <summary>
    /// Calculates the phase of this substance under the given conditions of temperature and
    /// pressure.
    /// </summary>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <returns>The phase of this substance under the given conditions.</returns>
    /// <remarks>
    /// Only the solid, liquid, and gas phases are considered. Exotic phases of matter must be
    /// indicated explicitly via <see cref="FixedPhase"/>; they will never be calculated by this
    /// method.
    /// </remarks>
    public PhaseType GetPhase(double temperature, double pressure)
    {
        if (FixedPhase.HasValue)
        {
            return FixedPhase.Value;
        }
        if (MeltingPoint.HasValue && temperature < MeltingPoint)
        {
            return PhaseType.Solid;
        }
        var vaporPressure = GetVaporPressure(temperature);
        if (vaporPressure.HasValue && pressure < vaporPressure.Value)
        {
            return PhaseType.Gas;
        }
        else if (!MeltingPoint.HasValue)
        {
            return PhaseType.Solid;
        }
        else
        {
            return PhaseType.Liquid;
        }
    }

    /// <summary>
    /// Gets an <see cref="ISubstanceReference"/> for this <see cref="HomogeneousSubstance"/>.
    /// </summary>
    /// <returns>An <see cref="ISubstanceReference"/> for this <see
    /// cref="HomogeneousSubstance"/>.</returns>
    public ISubstanceReference GetReference() => GetHomogeneousReference();

    /// <summary>
    /// Calculates the vapor pressure of this substance, in kPa.
    /// </summary>
    /// <param name="temperature">The temperature, in K.</param>
    /// <returns>The vapor pressure of this substance, in kPa, or <see langword="null"/> if the
    /// Antoine coefficients have not been set for this substance.</returns>
    /// <remarks>
    /// <para>
    /// Uses Antoine's equation. If Antoine coefficients have not been explicitly set for this
    /// chemical, the return value will be null.
    /// </para>
    /// <para>
    /// If the indicated <paramref name="temperature"/> is beyond the indicated range of the
    /// Antoine coefficients (via <see cref="AntoineMinimumTemperature"/> and/or <see
    /// cref="AntoineMaximumTemperature"/>), the result may be <see
    /// cref="double.PositiveInfinity"/> (if the temperature is above the maximum), or <see
    /// cref="double.NegativeInfinity"/> (if the temperature is below the minimum).
    /// </para>
    /// </remarks>
    public double? GetVaporPressure(double temperature)
    {
        if (AntoineMaximumTemperature.HasValue
            && temperature > AntoineMaximumTemperature)
        {
            return double.PositiveInfinity;
        }
        else if (AntoineMinimumTemperature.HasValue
            && temperature < AntoineMinimumTemperature)
        {
            return double.NegativeInfinity;
        }
        else if (AntoineCoefficientA.HasValue
            && AntoineCoefficientB.HasValue
            && AntoineCoefficientC.HasValue)
        {
            return Math.Pow(10, AntoineCoefficientA.Value - (AntoineCoefficientB.Value / (AntoineCoefficientC.Value + temperature))) * 100;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets a new substance without the given <paramref name="constituent"/>.
    /// </summary>
    /// <param name="constituent">A substance to remove.</param>
    /// <returns>A new substance without the given <paramref name="constituent"/>; may be
    /// empty.</returns>
    public ISubstance Remove(HomogeneousReference constituent) => Equals(constituent) ? None : this;

    /// <summary>
    /// Gets a new substance without the given <paramref name="constituent"/>.
    /// </summary>
    /// <param name="constituent">A substance to remove.</param>
    /// <returns>A new substance without the given <paramref name="constituent"/>; may be
    /// empty.</returns>
    public ISubstance Remove(IHomogeneous constituent) => Equals(constituent) ? None : this;

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
    public IEnumerable<(List<ISubstanceReference> components, decimal proportion)> SeparateByPhase(double temperature, double pressure, params PhaseType[] phases)
    {
        var substancePhase = GetPhase(temperature, pressure);
        var match = false;
        foreach (var phase in phases)
        {
            if ((substancePhase & phase) != PhaseType.None)
            {
                match = true;
                yield return (new List<ISubstanceReference> { GetReference() }, 1);
            }
        }
        yield return (new List<ISubstanceReference>(), 0);
        if (!match)
        {
            yield return (new List<ISubstanceReference> { GetReference() }, 1);
        }
    }

    /// <summary>Returns a string equivalent of this instance.</summary>
    /// <returns>A string equivalent of this instance.</returns>
    public override string ToString() => Name;

    /// <summary>
    /// Gets a copy of this instance with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">A new name for this instance.</param>
    /// <param name="commonNames">
    /// <para>
    /// An optional list of new common names for this substance.
    /// </para>
    /// <para>
    /// The list may be arranged in order of most to least common, so that the first name in the
    /// list (if a list is present at all) can be assumed to be the most recognizable name for the
    /// substance. However, this is not a strict requirement. Names may appear in any order,
    /// particularly if no specific usage data is available, or when various names are equally
    /// common in different contexts.
    /// </para>
    /// </param>
    /// <returns>A version of this instance with the given name.</returns>
    public ISubstance WithSubstanceName(string name, params string[] commonNames) => new HomogeneousSubstance(
        name,
        AntoineCoefficientA,
        AntoineCoefficientB,
        AntoineCoefficientC,
        AntoineMaximumTemperature,
        AntoineMinimumTemperature,
        DensityLiquid,
        DensitySolid,
        DensitySpecial,
        GreenhousePotential,
        Hardness,
        IsConductive,
        IsFlammable,
        IsGemstone,
        IsMetal,
        IsRadioactive,
        MeltingPoint,
        MolarMass,
        FixedPhase,
        YoungsModulus,
        commonNames);

    /// <summary>
    /// Indicates whether two substances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(HomogeneousSubstance left, ISubstance right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two substances are unequal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are unequal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(HomogeneousSubstance left, ISubstance right) => !(left == right);
}
