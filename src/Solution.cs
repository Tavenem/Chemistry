using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using Tavenem.DataStorage;

namespace Tavenem.Chemistry;

/// <summary>
/// A homogeneous combination of <see cref="IHomogeneous"/> components in particular
/// proportions, which shares a single phase.
/// </summary>
/// <remarks>
/// The phase of a solution can be defined according to defined values (i.e. <see
/// cref="AntoineCoefficientA"/>, <see cref="AntoineCoefficientB"/> <see
/// cref="AntoineCoefficientC"/>, <see cref="MeltingPoint"/>), but if not it will be determined
/// by the phase of its <see cref="Solvent"/>, which is the constituent chemical with the
/// highest proportion in the overall solution.
/// </remarks>
[TypeConverter(typeof(SubstanceConverter))]
public class Solution : IHomogeneous, IEquatable<Solution>
{
    /// <summary>
    /// A value used to generate a unique key for this <see cref="ISubstance"/>.
    /// </summary>
    public const ushort TypeKey = 3;

    private const string EmptyName = "Empty";
    /// <summary>
    /// An empty solution, containing no chemical constituents.
    /// </summary>
    public static readonly Solution Empty = new(Chemical.None);

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
    /// The upper limit of the Antoine coefficients' accuracy for this substance.
    /// It is presumed reasonable to assume that the substance always vaporizes
    /// above this temperature.
    /// </summary>
    public double? AntoineMaximumTemperature { get; }

    /// <summary>
    /// The lower limit of the Antoine coefficients' accuracy for this substance.
    /// It is presumed reasonable to assume that the substance always condenses
    /// below this temperature.
    /// </summary>
    public double? AntoineMinimumTemperature { get; }

    /// <summary>
    /// The collection of constituents that make up this solution, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </summary>
    [JsonConverter(typeof(SubstanceConstituentsConverter))]
    public IReadOnlyDictionary<HomogeneousReference, decimal> Constituents { get; }

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
    [JsonIgnore]
    public double GreenhousePotential => Constituents.Sum(x => x.Key.Homogeneous.GreenhousePotential * (double)x.Value);

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
    [JsonPropertyOrder(-2)]
    public string Id { get; }

    /// <summary>
    /// The <see cref="IIdItem.IdItemTypeName"/> for <see cref="Solution"/>.
    /// </summary>
    public const string SolutionIdItemTypeName = ":Solution:";
    /// <summary>
    /// A built-in, read-only type discriminator.
    /// </summary>
    [JsonPropertyOrder(-1)]
    public string IdItemTypeName => SolutionIdItemTypeName;

    /// <summary>
    /// Indicates whether this substance conducts electricity.
    /// </summary>
    public bool IsConductive { get; }

    /// <summary>
    /// Indicates whether this solution contains no constituents.
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Constituents.Count == 0;

    /// <summary>
    /// Indicates whether this substance is able to burn.
    /// </summary>
    public bool IsFlammable { get; }

    /// <summary>
    /// Indicates whether this substance is considered a gemstone.
    /// </summary>
    public bool IsGemstone { get; }

    /// <summary>
    /// Indicates whether this substance is a metal.
    /// </summary>
    [JsonIgnore]
    public bool IsMetal => Constituents.Average(x => x.Key.Homogeneous.IsMetal ? 1.0 : 0.0) >= 0.5;

    /// <summary>
    /// Indicates whether this substance is radioactive.
    /// </summary>
    [JsonIgnore]
    public bool IsRadioactive => Constituents.Any(x => x.Key.Homogeneous.IsRadioactive);

    /// <summary>
    /// The melting point of this substance at 100 kPa, in K.
    /// </summary>
    public double? MeltingPoint { get; }

    /// <summary>
    /// The molar mass of this substance, in kg/mol.
    /// </summary>
    [JsonIgnore]
    public double MolarMass => Constituents.Sum(x => x.Key.Homogeneous.MolarMass * (double)x.Value);

    /// <summary>
    /// A name for this solution.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The constituent of this solution with the highest proportion.
    /// </summary>
    /// <remarks>
    /// If the solution is not given explicit phase change data of its own, this constituent's
    /// phase will determine the phase of the entire solution.
    /// </remarks>
    [JsonIgnore]
    public HomogeneousReference Solvent { get; }

    /// <summary>
    /// <para>
    /// The Young's modulus of this solution, in GPa.
    /// </para>
    /// <para>
    /// May be <see langword="null"/>, which indicates no known value.
    /// </para>
    /// </summary>
    public double? YoungsModulus { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="constituents">
    /// <para>
    /// One or more chemicals to add to the solution, along with their relative proportions (as
    /// normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    public Solution(
        IEnumerable<(IHomogeneous substance, decimal proportion)> constituents,
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                new Dictionary<HomogeneousReference, decimal>(constituents
                    .GroupBy(x => x.substance.Id)
                    .ToDictionary(x => x.First().substance.GetHomogeneousReference(), x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion))))),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? (constituents.Any(x => x.substance.Hardness > 0)
                ? constituents
                    .Where(x => x.substance.Hardness > 0)
                    .Sum(x => x.substance.Hardness * (double)x.proportion / constituents.Count())
                : 0),
            isConductive ?? constituents.Sum(x => x.substance.IsMetal ? x.proportion / constituents.Count() : 0) >= 0.5m,
            isFlammable ?? constituents.Sum(x => x.substance.IsFlammable ? x.proportion / constituents.Count() : 0) >= 0.5m,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? (constituents.Any(x => x.substance.YoungsModulus.HasValue)
                ? constituents
                    .Where(x => x.substance.YoungsModulus.HasValue)
                    .Sum(x => x.substance.YoungsModulus!.Value * (double)x.proportion / constituents.Count())
                : (double?)null))
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="constituents">
    /// <para>
    /// One or more chemicals to add to the solution, along with their relative proportions (as
    /// normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    public Solution(
        IEnumerable<(HomogeneousReference substance, decimal proportion)> constituents,
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                new Dictionary<HomogeneousReference, decimal>(constituents
                    .GroupBy(x => x.substance.Id)
                    .ToDictionary(x => x.First().substance, x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion))))),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? (constituents.Any(x => x.substance.Homogeneous.Hardness > 0)
                ? constituents
                    .Where(x => x.substance.Homogeneous.Hardness > 0)
                    .Sum(x => x.substance.Homogeneous.Hardness * (double)x.proportion / constituents.Count())
                : 0),
            isConductive ?? constituents.Sum(x => x.substance.Homogeneous.IsMetal ? x.proportion / constituents.Count() : 0) >= 0.5m,
            isFlammable ?? constituents.Sum(x => x.substance.Homogeneous.IsFlammable ? x.proportion / constituents.Count() : 0) >= 0.5m,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? (constituents.Any(x => x.substance.Homogeneous.YoungsModulus.HasValue)
                ? constituents
                    .Where(x => x.substance.Homogeneous.YoungsModulus.HasValue)
                    .Sum(x => x.substance.Homogeneous.YoungsModulus!.Value * (double)x.proportion / constituents.Count())
                : (double?)null))
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="constituents">
    /// One or more chemicals to add to the solution, each of which will be included in equal
    /// proportions.
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    public Solution(
        IEnumerable<IHomogeneous> constituents,
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                new Dictionary<HomogeneousReference, decimal>(constituents
                    .GroupBy(x => x.Id)
                    .ToDictionary(x => x.First().GetHomogeneousReference(), x => x.Sum(_ => 1m / constituents.Count())))),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? (constituents.Any(x => x.Hardness > 0)
                ? constituents
                    .Where(x => x.Hardness > 0)
                    .Sum(x => x.Hardness / constituents.Count())
                : 0),
            isConductive ?? constituents.Sum(x => x.IsMetal ? 1m / constituents.Count() : 0) >= 0.5m,
            isFlammable ?? constituents.Sum(x => x.IsFlammable ? 1m / constituents.Count() : 0) >= 0.5m,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? (constituents.Any(x => x.YoungsModulus.HasValue)
                ? constituents
                    .Where(x => x.YoungsModulus.HasValue)
                    .Sum(x => x.YoungsModulus!.Value / constituents.Count())
                : (double?)null))
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="constituents">
    /// One or more chemicals to add to the solution, each of which will be included in equal
    /// proportions.
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    public Solution(
        IEnumerable<HomogeneousReference> constituents,
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                new Dictionary<HomogeneousReference, decimal>(constituents
                    .GroupBy(x => x.Id)
                    .ToDictionary(x => x.First(), x => x.Sum(_ => 1m / constituents.Count())))),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? (constituents.Any(x => x.Homogeneous.Hardness > 0)
                ? constituents
                    .Where(x => x.Homogeneous.Hardness > 0)
                    .Sum(x => x.Homogeneous.Hardness / constituents.Count())
                : 0),
            isConductive ?? constituents.Sum(x => x.Homogeneous.IsMetal ? 1m / constituents.Count() : 0) >= 0.5m,
            isFlammable ?? constituents.Sum(x => x.Homogeneous.IsFlammable ? 1m / constituents.Count() : 0) >= 0.5m,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? (constituents.Any(x => x.Homogeneous.YoungsModulus.HasValue)
                ? constituents
                    .Where(x => x.Homogeneous.YoungsModulus.HasValue)
                    .Sum(x => x.Homogeneous.YoungsModulus!.Value / constituents.Count())
                : (double?)null))
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    /// <param name="constituents">
    /// <para>
    /// One or more chemicals to add to the solution, along with their relative proportions (as
    /// normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    public Solution(
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null,
        params (IHomogeneous substance, decimal proportion)[] constituents) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                new Dictionary<HomogeneousReference, decimal>(constituents
                    .GroupBy(x => x.substance.Id)
                    .ToDictionary(x => x.First().substance.GetHomogeneousReference(), x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion))))),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? (constituents.Any(x => x.substance.Hardness > 0)
                ? constituents
                    .Where(x => x.substance.Hardness > 0)
                    .Sum(x => x.substance.Hardness * (double)x.proportion / constituents.Length)
                : 0),
            isConductive ?? constituents.Sum(x => x.substance.IsMetal ? x.proportion / constituents.Length : 0) >= 0.5m,
            isFlammable ?? constituents.Sum(x => x.substance.IsFlammable ? x.proportion / constituents.Length : 0) >= 0.5m,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? (constituents.Any(x => x.substance.YoungsModulus.HasValue)
                ? constituents
                    .Where(x => x.substance.YoungsModulus.HasValue)
                    .Sum(x => x.substance.YoungsModulus!.Value * (double)x.proportion / constituents.Length)
                : (double?)null))
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    /// <param name="constituents">
    /// <para>
    /// One or more chemicals to add to the solution, along with their relative proportions (as
    /// normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    public Solution(
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null,
        params (HomogeneousReference substance, decimal proportion)[] constituents) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                new Dictionary<HomogeneousReference, decimal>(constituents
                    .GroupBy(x => x.substance.Id)
                    .ToDictionary(x => x.First().substance, x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion))))),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? (constituents.Any(x => x.substance.Homogeneous.Hardness > 0)
                ? constituents
                    .Where(x => x.substance.Homogeneous.Hardness > 0)
                    .Sum(x => x.substance.Homogeneous.Hardness * (double)x.proportion / constituents.Length)
                : 0),
            isConductive ?? constituents.Sum(x => x.substance.Homogeneous.IsMetal ? x.proportion / constituents.Length : 0) >= 0.5m,
            isFlammable ?? constituents.Sum(x => x.substance.Homogeneous.IsFlammable ? x.proportion / constituents.Length : 0) >= 0.5m,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? (constituents.Any(x => x.substance.Homogeneous.YoungsModulus.HasValue)
                ? constituents
                    .Where(x => x.substance.Homogeneous.YoungsModulus.HasValue)
                    .Sum(x => x.substance.Homogeneous.YoungsModulus!.Value * (double)x.proportion / constituents.Length)
                : (double?)null))
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    /// <param name="chemicals">
    /// One or more chemicals to add to the solution, each of which will be included in equal
    /// proportions.
    /// </param>
    public Solution(
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null,
        params IHomogeneous[] chemicals) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                new Dictionary<HomogeneousReference, decimal>(chemicals
                    .GroupBy(x => x.Id)
                    .ToDictionary(x => x.First().GetHomogeneousReference(), x => x.Sum(_ => 1m / chemicals.Length)))),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? (chemicals.Any(x => x.Hardness > 0)
                ? chemicals
                    .Where(x => x.Hardness > 0)
                    .Sum(x => x.Hardness / chemicals.Length)
                : 0),
            isConductive ?? chemicals.Sum(x => x.IsMetal ? 1m / chemicals.Length : 0) >= 0.5m,
            isFlammable ?? chemicals.Sum(x => x.IsFlammable ? 1m / chemicals.Length : 0) >= 0.5m,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? (chemicals.Any(x => x.YoungsModulus.HasValue)
                ? chemicals
                    .Where(x => x.YoungsModulus.HasValue)
                    .Sum(x => x.YoungsModulus!.Value / chemicals.Length)
                : (double?)null))
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    /// <param name="chemicals">
    /// One or more chemicals to add to the solution, each of which will be included in equal
    /// proportions.
    /// </param>
    public Solution(
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null,
        params HomogeneousReference[] chemicals) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                new Dictionary<HomogeneousReference, decimal>(chemicals
                    .GroupBy(x => x.Id)
                    .ToDictionary(x => x.First(), x => x.Sum(_ => 1m / chemicals.Length)))),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? (chemicals.Any(x => x.Homogeneous.Hardness > 0)
                ? chemicals
                    .Where(x => x.Homogeneous.Hardness > 0)
                    .Sum(x => x.Homogeneous.Hardness / chemicals.Length)
                : 0),
            isConductive ?? chemicals.Sum(x => x.Homogeneous.IsMetal ? 1m / chemicals.Length : 0) >= 0.5m,
            isFlammable ?? chemicals.Sum(x => x.Homogeneous.IsFlammable ? 1m / chemicals.Length : 0) >= 0.5m,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? (chemicals.Any(x => x.Homogeneous.YoungsModulus.HasValue)
                ? chemicals
                    .Where(x => x.Homogeneous.YoungsModulus.HasValue)
                    .Sum(x => x.Homogeneous.YoungsModulus!.Value / chemicals.Length)
                : (double?)null))
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="substance">
    /// A single chemical which will comprise the entire "solution."
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    public Solution(
        IHomogeneous substance,
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(new Dictionary<HomogeneousReference, decimal> { { substance.GetHomogeneousReference(), 1 } }),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? substance.Hardness,
            isConductive ?? substance.IsConductive,
            isFlammable ?? substance.IsFlammable,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? substance.YoungsModulus)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="substance">
    /// A single chemical which will comprise the entire "solution."
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    public Solution(
        HomogeneousReference substance,
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(new Dictionary<HomogeneousReference, decimal> { { substance, 1 } }),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? substance.Homogeneous.Hardness,
            isConductive ?? substance.Homogeneous.IsConductive,
            isFlammable ?? substance.Homogeneous.IsFlammable,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? substance.Homogeneous.YoungsModulus)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="id">The unique ID of this substance.</param>
    /// <param name="idItemTypeName">The type discriminator.</param>
    /// <param name="constituents">
    /// <para>
    /// One or more chemicals to add to the solution, along with their relative proportions (as
    /// normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    [JsonConstructor]
    public Solution(
        string id,
        string idItemTypeName,
        IReadOnlyDictionary<HomogeneousReference, decimal> constituents,
        string name,
        double? antoineCoefficientA,
        double? antoineCoefficientB,
        double? antoineCoefficientC,
        double? antoineMaximumTemperature,
        double? antoineMinimumTemperature,
        double? densityLiquid,
        double? densitySolid,
        double? densitySpecial,
        double hardness,
        bool isConductive,
        bool isFlammable,
        bool isGemstone,
        double? meltingPoint,
        PhaseType? fixedPhase,
        double? youngsModulus)
    {
        Id = id;
        Constituents = constituents;
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
        Hardness = hardness;
        IsConductive = isConductive;
        IsFlammable = isFlammable;
        IsGemstone = isGemstone;
        MeltingPoint = meltingPoint;
        FixedPhase = fixedPhase;
        Solvent = Constituents.Count == 0
            ? HomogeneousReference.Empty
            : Constituents.OrderByDescending(x => x.Value).First().Key;
        YoungsModulus = youngsModulus;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="id">The unique ID of this substance.</param>
    /// <param name="constituents">
    /// <para>
    /// One or more chemicals to add to the solution, along with their relative proportions (as
    /// normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    internal Solution(
        string id,
        IEnumerable<(HomogeneousReference substance, decimal proportion)> constituents,
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null) : this(
            id,
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                new Dictionary<HomogeneousReference, decimal>(constituents
                    .GroupBy(x => x.substance.Id)
                    .ToDictionary(x => x.First().substance, x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion))))),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? (constituents.Any(x => x.substance.Homogeneous.Hardness > 0)
                ? constituents
                    .Where(x => x.substance.Homogeneous.Hardness > 0)
                    .Sum(x => x.substance.Homogeneous.Hardness * (double)x.proportion / constituents.Count())
                : 0),
            isConductive ?? constituents.Sum(x => x.substance.Homogeneous.IsMetal ? x.proportion / constituents.Count() : 0) >= 0.5m,
            isFlammable ?? constituents.Sum(x => x.substance.Homogeneous.IsFlammable ? x.proportion / constituents.Count() : 0) >= 0.5m,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? (constituents.Any(x => x.substance.Homogeneous.YoungsModulus.HasValue)
                ? constituents
                    .Where(x => x.substance.Homogeneous.YoungsModulus.HasValue)
                    .Sum(x => x.substance.Homogeneous.YoungsModulus!.Value * (double)x.proportion / constituents.Count())
                : (double?)null))
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Solution"/>.
    /// </summary>
    /// <param name="id">The unique ID of this substance.</param>
    /// <param name="substance">
    /// A single chemical which will comprise the entire "solution."
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this solution.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a solution name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
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
    /// A maximum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// A minimum Antoine temperature, in K. Can be omitted to default to that of the solvent.
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
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="hardness">The hardness of the chemical as a solid, in MPa. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isConductive">
    /// <para>
    /// Whether or not the chemical is conductive.
    /// </para>
    /// <para>
    /// If unspecified, assumed to be <see langword="true"/> for metals.
    /// </para>
    /// </param>
    /// <param name="isFlammable">Whether or not the chemical is flammable. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="isGemstone">Whether this substance is considered a gemstone.</param>
    /// <param name="meltingPoint">
    /// A melting point, in K. If omitted, the weighted average value of the constituents is used.
    /// </param>
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
    internal Solution(
        string id,
        HomogeneousReference substance,
        string? name = null,
        double? antoineCoefficientA = null,
        double? antoineCoefficientB = null,
        double? antoineCoefficientC = null,
        double? antoineMaximumTemperature = null,
        double? antoineMinimumTemperature = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        double? hardness = null,
        bool? isConductive = null,
        bool? isFlammable = null,
        bool isGemstone = false,
        double? meltingPoint = null,
        PhaseType? fixedPhase = null,
        double? youngsModulus = null) : this(
            id,
            new ReadOnlyDictionary<HomogeneousReference, decimal>(new Dictionary<HomogeneousReference, decimal> { { substance, 1 } }),
            name,
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness ?? substance.Homogeneous.Hardness,
            isConductive ?? substance.Homogeneous.IsConductive,
            isFlammable ?? substance.Homogeneous.IsFlammable,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus ?? substance.Homogeneous.YoungsModulus)
    { }

    private Solution(
        string id,
        IReadOnlyDictionary<HomogeneousReference, decimal> constituents,
        string? name,
        double? antoineCoefficientA,
        double? antoineCoefficientB,
        double? antoineCoefficientC,
        double? antoineMaximumTemperature,
        double? antoineMinimumTemperature,
        double? densityLiquid,
        double? densitySolid,
        double? densitySpecial,
        double hardness,
        bool isConductive,
        bool isFlammable,
        bool isGemstone,
        double? meltingPoint,
        PhaseType? fixedPhase,
        double? youngsModulus) : this(
            id,
            SolutionIdItemTypeName,
            constituents,
            name ?? GetName(constituents),
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness,
            isConductive,
            isFlammable,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus)
    { }

    private Solution(
        IReadOnlyDictionary<HomogeneousReference, decimal> constituents,
        string? name,
        double? antoineCoefficientA,
        double? antoineCoefficientB,
        double? antoineCoefficientC,
        double? antoineMaximumTemperature,
        double? antoineMinimumTemperature,
        double? densityLiquid,
        double? densitySolid,
        double? densitySpecial,
        double hardness,
        bool isConductive,
        bool isFlammable,
        bool isGemstone,
        double? meltingPoint,
        PhaseType? fixedPhase,
        double? youngsModulus) : this(
            Guid.NewGuid().ToString(),
            SolutionIdItemTypeName,
            constituents,
            name ?? GetName(constituents),
            antoineCoefficientA,
            antoineCoefficientB,
            antoineCoefficientC,
            antoineMaximumTemperature,
            antoineMinimumTemperature,
            densityLiquid,
            densitySolid,
            densitySpecial,
            hardness,
            isConductive,
            isFlammable,
            isGemstone,
            meltingPoint,
            fixedPhase,
            youngsModulus)
    { }

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
    public ISubstance AddConstituent(HomogeneousReference constituent, decimal proportion = 0.5m)
    {
        if (proportion <= 0)
        {
            return this;
        }
        if (proportion >= 1)
        {
            return constituent.Homogeneous;
        }
        var included = Constituents.Any(x => x.Key.Equals(constituent));
        var match = included ? Constituents.First(x => x.Key.Equals(constituent)) : (KeyValuePair<HomogeneousReference, decimal>?)null;
        var ratio = included ? 1 - (proportion - match!.Value.Value) : 1 - proportion;
        var newConstituents = new List<(HomogeneousReference, decimal)>();
        foreach (var keyValuePair in Constituents.ToList())
        {
            if (included && keyValuePair.Key.Equals(match!.Value.Key))
            {
                newConstituents.Add((keyValuePair.Key, proportion));
            }
            else
            {
                newConstituents.Add((keyValuePair.Key, keyValuePair.Value * ratio));
            }
        }
        if (!included)
        {
            newConstituents.Add((constituent, proportion));
        }
        return new Solution(newConstituents);
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
    public ISubstance AddConstituent(IHomogeneous constituent, decimal proportion = 0.5m)
    {
        if (proportion <= 0)
        {
            return this;
        }
        if (proportion >= 1)
        {
            return constituent;
        }
        var key = constituent.GetHomogeneousReference();
        var included = Constituents.Any(x => x.Key.Equals(key));
        var match = included ? Constituents.First(x => x.Key.Equals(key)) : (KeyValuePair<HomogeneousReference, decimal>?)null;
        var ratio = included ? 1 - (proportion - match!.Value.Value) : 1 - proportion;
        var newConstituents = new List<(HomogeneousReference, decimal)>();
        foreach (var keyValuePair in Constituents.ToList())
        {
            if (included && keyValuePair.Key.Equals(match!.Value.Key))
            {
                newConstituents.Add((keyValuePair.Key, proportion));
            }
            else
            {
                newConstituents.Add((keyValuePair.Key, keyValuePair.Value * ratio));
            }
        }
        if (!included)
        {
            newConstituents.Add((key, proportion));
        }
        return new Solution(newConstituents);
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
    /// proportionately to accomodate this value.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="substance"/>.</returns>
    /// <remarks>
    /// <para>
    /// The result of combining a solution with any substance other than a chemical or a
    /// solution with the same solvent will be of the other substance's type. For example,
    /// combining a solution and a mixture will result in a mixture.
    /// </para>
    /// <para>
    /// In order to instead produce a solution which contains the original constituents of this
    /// instance, in addition to the constituents of another substance, use <see
    /// cref="Dissolve(ISubstance, decimal)"/> instead.
    /// </para>
    /// </remarks>
    public ISubstance Combine(ISubstanceReference substance, decimal proportion = 0.5m)
    {
        if (proportion <= 0)
        {
            return this;
        }
        if (proportion >= 1)
        {
            return substance.Substance;
        }

        var s = substance.Substance;
        if (s is Chemical chemical)
        {
            return AddConstituent(chemical, proportion);
        }

        if (s is Solution other
            && other.Solvent.Equals(Solvent))
        {
            return Dissolve(other, proportion);
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
    /// proportionately to accomodate this value.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="ISubstance"/> instance representing the combination of this
    /// instance with the given <paramref name="substance"/>.</returns>
    /// <remarks>
    /// <para>
    /// The result of combining a solution with any substance other than a chemical or a
    /// solution with the same solvent will be of the other substance's type. For example,
    /// combining a solution and a mixture will result in a mixture.
    /// </para>
    /// <para>
    /// In order to instead produce a solution which contains the original constituents of this
    /// instance, in addition to the constituents of another substance, use <see
    /// cref="Dissolve(ISubstance, decimal)"/> instead.
    /// </para>
    /// </remarks>
    public ISubstance Combine(ISubstance substance, decimal proportion = 0.5m)
    {
        if (proportion <= 0)
        {
            return this;
        }
        if (proportion >= 1)
        {
            return substance;
        }

        if (substance is Chemical chemical)
        {
            return AddConstituent(chemical, proportion);
        }

        if (substance is Solution other
            && other.Solvent.Equals(Solvent))
        {
            return Dissolve(other, proportion);
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
        => Constituents.Any(x => x.Key.Equals(substance)
        && (phase == PhaseType.Any || (x.Key.Homogeneous.GetPhase(temperature, pressure) & phase) != PhaseType.None));

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
        => Constituents.Any(x => x.Key.Equals(substance.GetReference())
        && (phase == PhaseType.Any || (x.Key.Homogeneous.GetPhase(temperature, pressure) & phase) != PhaseType.None));

    /// <summary>
    /// Dissolves all constituents of the given <paramref name="substance"/> into this solution.
    /// </summary>
    /// <param name="substance">An <see cref="ISubstance"/> instance to dissolve into this
    /// solution.</param>
    /// <param name="proportion">
    /// <para>
    /// The proportion at which to add the given <paramref name="substance"/>.
    /// </para>
    /// <para>
    /// The proportions of the individual constituents of each substance will be reduced
    /// proportionately to accomodate this value. Note that dissolving another substance at too
    /// high a proportion may result in a new constituent becoming the de facto solvent.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="Solution"/> instance representing the combination of this
    /// instance with the given <paramref name="substance"/>.</returns>
    public ISubstance Dissolve(ISubstanceReference substance, decimal proportion = 0.5m)
    {
        if (proportion <= 0)
        {
            return this;
        }
        if (proportion >= 1)
        {
            return substance.Substance;
        }

        if (substance is IHomogeneous chemical)
        {
            return AddConstituent(chemical, proportion);
        }

        var allChemicals = new Dictionary<HomogeneousReference, decimal>();
        var ratio = 1 - proportion;
        foreach (var component in Constituents)
        {
            allChemicals.Add(component.Key, component.Value * ratio);
        }
        foreach (var other in substance.Substance.Constituents)
        {
            if (allChemicals.ContainsKey(other.Key))
            {
                allChemicals[other.Key] += other.Value * proportion;
            }
            else
            {
                allChemicals.Add(other.Key, other.Value * proportion);
            }
        }
        return new Solution(allChemicals.Select(x => (x.Key, x.Value)));
    }

    /// <summary>
    /// Dissolves all constituents of the given <paramref name="substance"/> into this solution.
    /// </summary>
    /// <param name="substance">An <see cref="ISubstance"/> instance to dissolve into this
    /// solution.</param>
    /// <param name="proportion">
    /// <para>
    /// The proportion at which to add the given <paramref name="substance"/>.
    /// </para>
    /// <para>
    /// The proportions of the individual constituents of each substance will be reduced
    /// proportionately to accomodate this value. Note that dissolving another substance at too
    /// high a proportion may result in a new constituent becoming the de facto solvent.
    /// </para>
    /// </param>
    /// <returns>A new <see cref="Solution"/> instance representing the combination of this
    /// instance with the given <paramref name="substance"/>.</returns>
    public ISubstance Dissolve(ISubstance substance, decimal proportion = 0.5m)
    {
        if (proportion <= 0)
        {
            return this;
        }
        if (proportion >= 1)
        {
            return substance;
        }

        if (substance is IHomogeneous chemical)
        {
            return AddConstituent(chemical, proportion);
        }

        var allChemicals = new Dictionary<HomogeneousReference, decimal>();
        var ratio = 1 - proportion;
        foreach (var component in Constituents)
        {
            allChemicals.Add(component.Key, component.Value * ratio);
        }
        foreach (var other in substance.Constituents)
        {
            if (allChemicals.ContainsKey(other.Key))
            {
                allChemicals[other.Key] += other.Value * proportion;
            }
            else
            {
                allChemicals.Add(other.Key, other.Value * proportion);
            }
        }
        return new Solution(allChemicals.Select(x => (x.Key, x.Value)));
    }

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Solution? other) => other is not null && Id.Equals(other.Id);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(IIdItem? other)
        => other is Solution solution && Equals(solution);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ISubstance? other)
        => other is Solution solution && Equals(solution);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(HomogeneousReference? other)
        => other is HomogeneousReference reference && reference.Equals(this);

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
        var hashSet = new HashSet<int>();
        foreach (var constituent in Constituents)
        {
            foreach (var chemical in constituent.Key.Homogeneous.GetChemicalConstituents())
            {
                var hash = chemical.GetHashCode();
                if (!hashSet.Contains(hash))
                {
                    hashSet.Add(hash);
                    yield return chemical;
                }
            }
        }
    }

    /// <summary>
    /// Gets the approximate average density of this substance under the given conditions of
    /// <paramref name="temperature"/> and <paramref name="pressure"/>, in kg/m³.
    /// </summary>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <returns>The approximate average density of this substance under the given conditions of
    /// <paramref name="temperature"/> and <paramref name="pressure"/>, in kg/m³.</returns>
    public double GetDensity(double temperature, double pressure)
    {
        if (DensitySolid.HasValue || DensityLiquid.HasValue || DensitySpecial.HasValue)
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

        return Constituents.Sum(x => x.Key.Homogeneous.GetDensity(temperature, pressure) * (double)x.Value);
    }

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets an <see cref="HomogeneousReference"/> for this <see cref="HomogeneousSubstance"/>.
    /// </summary>
    /// <returns>An <see cref="HomogeneousReference"/> for this <see
    /// cref="HomogeneousSubstance"/>.</returns>
    public HomogeneousReference GetHomogeneousReference() => new(this);

    /// <summary>
    /// <para>
    /// In heterogeneous composites, gets a homogenized version of the solution.
    /// </para>
    /// <para>
    /// In a <see cref="Solution"/>, gets the instance, unchanged.
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
        var solvent = Solvent.Homogeneous;
        var fixedPhase = FixedPhase ?? solvent.FixedPhase;
        if (fixedPhase.HasValue)
        {
            return fixedPhase.Value;
        }
        var meltingPoint = MeltingPoint ?? solvent.MeltingPoint;
        if (meltingPoint.HasValue && temperature < meltingPoint)
        {
            return PhaseType.Solid;
        }
        var vaporPressure = GetVaporPressure(temperature);
        if (vaporPressure.HasValue && pressure < vaporPressure.Value)
        {
            return PhaseType.Gas;
        }
        else if (!meltingPoint.HasValue)
        {
            return PhaseType.Solid;
        }
        else
        {
            return PhaseType.Liquid;
        }
    }

    /// <summary>
    /// Gets the proportion of the given <paramref name="constituent"/> in this substance.
    /// </summary>
    /// <param name="constituent">An <see cref="IHomogeneous"/> constituent whose proportion in
    /// this instance will be determined.</param>
    /// <returns>The proportion of the given <paramref name="constituent"/> in this substance;
    /// or zero, if it does not contain the given <paramref name="constituent"/>.</returns>
    public decimal GetProportion(HomogeneousReference constituent)
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
    public decimal GetProportion(IHomogeneous constituent)
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
    public decimal GetProportion(Predicate<IHomogeneous> condition)
        => this is IHomogeneous h && condition.Invoke(h)
        ? 1
        : Constituents?.Sum(x => condition.Invoke(x.Key.Homogeneous) ? x.Value : 0) ?? 0;

    /// <summary>
    /// Gets an <see cref="ISubstanceReference"/> for this <see cref="Solution"/>.
    /// </summary>
    /// <returns>An <see cref="ISubstanceReference"/> for this <see
    /// cref="Solution"/>.</returns>
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
        var solvent = Solvent.Homogeneous;
        var antoineMaximumTemperature = AntoineMaximumTemperature ?? solvent.AntoineMaximumTemperature;
        if (antoineMaximumTemperature.HasValue
            && temperature > antoineMaximumTemperature)
        {
            return double.PositiveInfinity;
        }
        var antoineMinimumTemperature = AntoineMinimumTemperature ?? solvent.AntoineMinimumTemperature;
        if (antoineMinimumTemperature.HasValue
            && temperature < antoineMinimumTemperature)
        {
            return double.NegativeInfinity;
        }
        var antoineCoefficientA = AntoineCoefficientA ?? solvent.AntoineCoefficientA;
        var antoineCoefficientB = AntoineCoefficientB ?? solvent.AntoineCoefficientB;
        var antoineCoefficientC = AntoineCoefficientC ?? solvent.AntoineCoefficientC;
        if (antoineCoefficientA.HasValue
            && antoineCoefficientB.HasValue
            && antoineCoefficientC.HasValue)
        {
            return Math.Pow(10, antoineCoefficientA.Value - (antoineCoefficientB.Value / (antoineCoefficientC.Value + temperature))) * 100;
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
    public ISubstance Remove(HomogeneousReference constituent)
    {
        if (!Constituents.ContainsKey(constituent))
        {
            return this;
        }
        var list = Constituents.Where(x => x.Key != constituent).ToList();
        if (list.Count == 1)
        {
            return list[0].Key.Substance;
        }
        var ratio = 1 / list.Sum(x => x.Value);
        for (var i = 0; i < list.Count; i++)
        {
            list[i] = new KeyValuePair<HomogeneousReference, decimal>(list[i].Key, list[i].Value * ratio);
        }
        return new Solution(list.Select(x => (x.Key, x.Value)));
    }

    /// <summary>
    /// Gets a new substance without the given <paramref name="constituent"/>.
    /// </summary>
    /// <param name="constituent">A substance to remove.</param>
    /// <returns>A new substance without the given <paramref name="constituent"/>; may be
    /// empty.</returns>
    public ISubstance Remove(IHomogeneous constituent)
    {
        var key = constituent.GetHomogeneousReference();
        if (!Constituents.ContainsKey(key))
        {
            return this;
        }
        var list = Constituents.Where(x => x.Key != key).ToList();
        if (list.Count == 1)
        {
            return list[0].Key.Substance;
        }
        var ratio = 1 / list.Sum(x => x.Value);
        for (var i = 0; i < list.Count; i++)
        {
            list[i] = new KeyValuePair<HomogeneousReference, decimal>(list[i].Key, list[i].Value * ratio);
        }
        return new Solution(list.Select(x => (x.Key, x.Value)));
    }

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
        var constituentPhases = Constituents.Select(x => (substance: x.Key, proportion: x.Value, phase: x.Key.Homogeneous.GetPhase(temperature, pressure))).ToList();
        var allMatched = new HashSet<int>();
        foreach (var phase in phases)
        {
            var matchedList = constituentPhases.Where(x => (x.phase & phase) != PhaseType.None).ToList();
            foreach (var match in matchedList)
            {
                allMatched.Add(match.substance.GetHashCode());
            }
            var matchedProportion = matchedList.Sum(x => x.proportion);
            yield return (matchedList.Select(x => x.substance as ISubstanceReference).ToList(), matchedProportion);
        }
        var unmatchedList = Constituents.Where(x => !allMatched.Contains(x.Key.GetHashCode())).ToList();
        var unmatchedProportion = unmatchedList.Sum(x => x.Value);
        yield return (unmatchedList.Select(x => x.Key as ISubstanceReference).ToList(), unmatchedProportion);
    }

    /// <summary>Returns a string equivalent of this instance.</summary>
    /// <returns>A string equivalent of this instance.</returns>
    public override string ToString() => Name;

    /// <summary>
    /// Gets a copy of this instance with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">A new name for this instance.</param>
    /// <returns>A version of this instance with the given name.</returns>
    public ISubstance WithSubstanceName(string name) => new Solution(
        Constituents,
        name,
        AntoineCoefficientA,
        AntoineCoefficientB,
        AntoineCoefficientC,
        AntoineMaximumTemperature,
        AntoineMinimumTemperature,
        DensityLiquid,
        DensitySolid,
        DensitySpecial,
        Hardness,
        IsConductive,
        IsFlammable,
        IsGemstone,
        MeltingPoint,
        FixedPhase,
        YoungsModulus);

    private static string GetName(IReadOnlyDictionary<HomogeneousReference, decimal> chemicals)
    {
        if (chemicals.Count == 0)
        {
            return EmptyName;
        }
        var sb = new StringBuilder();
        foreach (var chemical in chemicals)
        {
            if (sb.Length > 0)
            {
                sb.Append("; ");
            }
            sb.Append(chemical.Key.Homogeneous.Name)
                .Append(':')
                .Append(chemical.Value.ToString("P3"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Indicates whether two substances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(Solution left, ISubstance right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two substances are unequal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are unequal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(Solution left, ISubstance right) => !(left == right);
}
