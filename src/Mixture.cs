using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using Tavenem.DataStorage;
using Tavenem.Mathematics;

namespace Tavenem.Chemistry;

/// <summary>
/// A heterogeneous combination of constituent components in particular proportions.
/// </summary>
/// <remarks>
/// Each constituent of a mixture is a homogeneous component, including chemicals, compounds,
/// and homogeneous solutions. A "mixture of mixtures" is modeled simply as a single mixture
/// containing all chemically indivisible constituents in their proper proportions.
/// </remarks>
[TypeConverter(typeof(SubstanceConverter))]
public class Mixture : ISubstance, IEquatable<Mixture>
{
    /// <summary>
    /// An empty mixture, containing no chemical constituents.
    /// </summary>
    public static readonly Mixture Empty = new(Chemical.None);

    /// <summary>
    /// The collection of constituents that make up this mixture, along with their relative
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
    [JsonIgnore]
    public double Hardness => Constituents.Any(x => x.Key.Homogeneous.Hardness > 0)
        ? Constituents.Where(x => x.Key.Homogeneous.Hardness > 0).Sum(x => x.Key.Homogeneous.Hardness * (double)x.Value)
        : 0;

    /// <summary>
    /// The ID of this item.
    /// </summary>
    [JsonPropertyName("id"), JsonPropertyOrder(-1)]
    public string Id { get; }

    /// <summary>
    /// The <see cref="IIdItem.IdItemTypeName"/> for <see cref="Mixture"/>.
    /// </summary>
    public const string MixtureIdItemTypeName = ":Mixture:";
    /// <summary>
    /// A built-in, read-only type discriminator.
    /// </summary>
    [JsonPropertyName("$type"), JsonPropertyOrder(-2)]
    public string IdItemTypeName => MixtureIdItemTypeName;

    /// <summary>
    /// Indicates whether this substance conducts electricity.
    /// </summary>
    [JsonIgnore]
    public bool IsConductive => Constituents.Average(x => x.Key.Homogeneous.IsConductive ? 1.0 : 0.0) >= 0.5;

    /// <summary>
    /// Indicates whether this mixture contains no constituents.
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Constituents.Count == 0;

    /// <summary>
    /// Indicates whether this substance is able to burn.
    /// </summary>
    [JsonIgnore]
    public bool IsFlammable => Constituents.Average(x => x.Key.Homogeneous.IsFlammable ? 1.0 : 0.0) >= 0.5;

    /// <summary>
    /// <para>
    /// Indicates whether this substance is considered a gemstone.
    /// </para>
    /// <para>
    /// Considered <see langword="true"/> for a <see cref="Mixture"/> only if all constituents
    /// are gemstones.
    /// </para>
    /// </summary>
    [JsonIgnore]
    public bool IsGemstone => Constituents.All(x => x.Key.Homogeneous.IsGemstone);

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
    /// The molar mass of this substance, in kg/mol.
    /// </summary>
    [JsonIgnore]
    public double MolarMass => Constituents.Sum(x => x.Key.Homogeneous.MolarMass * (double)x.Value);

    /// <summary>
    /// A name for this mixture.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// <para>
    /// The Young's modulus of this chemical, in GPa.
    /// </para>
    /// <para>
    /// May be <see langword="null"/>, which indicates no known value.
    /// </para>
    /// </summary>
    [JsonIgnore]
    public double? YoungsModulus => Constituents.Any(x => x.Key.Homogeneous.YoungsModulus.HasValue)
        ? Constituents.Where(x => x.Key.Homogeneous.YoungsModulus.HasValue).Sum(x => x.Key.Homogeneous.YoungsModulus!.Value * (double)x.Value)
        : null;

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="constituents">
    /// <para>
    /// One or more homogeneous constituents to add to the mixture, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    public Mixture(
        IEnumerable<(IHomogeneous constituent, decimal proportion)> constituents,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.GroupBy(x => x.constituent.Id)
                .ToDictionary(x => x.First().constituent.GetHomogeneousReference(), x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion)))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="constituents">
    /// <para>
    /// One or more homogeneous constituents to add to the mixture, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    public Mixture(
        IEnumerable<(HomogeneousReference constituent, decimal proportion)> constituents,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.GroupBy(x => x.constituent.Id)
                .ToDictionary(x => x.First().constituent, x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion)))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="constituents">
    /// One or more homogeneous constituents to add to the mixture, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    public Mixture(
        IEnumerable<IHomogeneous> constituents,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.GroupBy(x => x.Id).ToDictionary(x => x.First().GetHomogeneousReference(), x => x.Sum(_ => 1m / constituents.Count()))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="constituents">
    /// One or more homogeneous constituents to add to the mixture, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    public Mixture(
        IEnumerable<HomogeneousReference> constituents,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.GroupBy(x => x.Id).ToDictionary(x => x.First(), x => x.Sum(_ => 1m / constituents.Count()))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="constituents">
    /// One or more constituents to add to the mixture, along with their relative proportions
    /// (as normalized values between zero and one).
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    public Mixture(
        IEnumerable<Mixture> constituents,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.SelectMany(x => x.Constituents).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value / constituents.Count()))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="constituents">
    /// <para>
    /// One or more homogeneous constituents to add to the mixture, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    public Mixture(
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        params (IHomogeneous constituent, decimal proportion)[] constituents) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.GroupBy(x => x.constituent.Id)
                .ToDictionary(x => x.First().constituent.GetHomogeneousReference(), x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion)))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="constituents">
    /// <para>
    /// One or more homogeneous constituents to add to the mixture, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    public Mixture(
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        params (HomogeneousReference constituent, decimal proportion)[] constituents) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.GroupBy(x => x.constituent.Id)
                .ToDictionary(x => x.First().constituent, x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion)))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="constituents">
    /// <para>
    /// One or more constituents to add to the mixture, along with their relative proportions
    /// (as normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    public Mixture(
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        params (Mixture constituent, decimal proportion)[] constituents) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.SelectMany(x => x.constituent.Constituents.Select(y => (c: y.Key, p: x.proportion * y.Value)))
                .GroupBy(x => x.c.Id)
                .ToDictionary(x => x.First().c, x => x.Sum(y => y.p / constituents.Sum(z => z.proportion)))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="constituents">
    /// One or more homogeneous chemical constituents to add to the mixture, each of which will
    /// be included in equal
    /// proportions.
    /// </param>
    public Mixture(
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        params IHomogeneous[] constituents) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.GroupBy(x => x.Id).ToDictionary(x => x.First().GetHomogeneousReference(), x => x.Sum(_ => 1m / constituents.Length))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="constituents">
    /// One or more homogeneous chemical constituents to add to the mixture, each of which will
    /// be included in equal
    /// proportions.
    /// </param>
    public Mixture(
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        params HomogeneousReference[] constituents) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.GroupBy(x => x.Id).ToDictionary(x => x.First(), x => x.Sum(_ => 1m / constituents.Length))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    /// <param name="constituents">
    /// One or more constituents to add to the mixture, each of which will be included in equal
    /// proportions.
    /// </param>
    public Mixture(
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null,
        params Mixture[] constituents) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.SelectMany(x => x.Constituents).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value / constituents.Length))),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="constituent">
    /// A single homogeneous chemical constituent which will comprise the entire "mixture."
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    public Mixture(
        IHomogeneous constituent,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(new Dictionary<HomogeneousReference, decimal> { { constituent.GetHomogeneousReference(), 1 } }),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="constituent">
    /// A single homogeneous chemical constituent which will comprise the entire "mixture."
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    public Mixture(
        HomogeneousReference constituent,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(new Dictionary<HomogeneousReference, decimal> { { constituent, 1 } }),
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="id">The unique ID of this substance.</param>
    /// <param name="constituents">
    /// <para>
    /// One or more homogeneous constituents to add to the mixture, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    [JsonConstructor]
    public Mixture(
        string id,
        IReadOnlyDictionary<HomogeneousReference, decimal> constituents,
        string name,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null)
    {
        Id = id;
        Constituents = constituents;
        Name = name;
        DensityLiquid = densityLiquid;
        DensitySolid = densitySolid;
        DensitySpecial = densitySpecial;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Mixture"/>.
    /// </summary>
    /// <param name="id">The unique ID of this substance.</param>
    /// <param name="constituents">
    /// <para>
    /// One or more homogeneous constituents to add to the mixture, along with their relative
    /// proportions (as normalized values between zero and one).
    /// </para>
    /// <para>
    /// If the proportion values are not normalized (do not sum to 1), they will be normalized
    /// during initialization.
    /// </para>
    /// </param>
    /// <param name="name">
    /// <para>
    /// A name for this mixture.
    /// </para>
    /// <para>
    /// If omitted a name based on the constituents will be generated in the following form:
    /// "Oxygen:25.500%; Nitrogen:74.500%".
    /// </para>
    /// <para>
    /// Note that chemical names may also be auto-generated from the Hill notation of their
    /// chemical formula if not explicitly given, which may lead to a mixture name such as:
    /// "H₂O:96.240%; NaCl:3.760%".
    /// </para>
    /// </param>
    /// <param name="densityLiquid">The approximate density of the chemical in the liquid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySolid">The approximate density of the chemical in the solid phase,
    /// in kg/m³. If omitted, the weighted average value of the constituents is used.</param>
    /// <param name="densitySpecial">The approximate density of this substance when its phase is
    /// neither solid, liquid, nor gas, in kg/m³.</param>
    internal Mixture(
        string id,
        IEnumerable<(HomogeneousReference constituent, decimal proportion)> constituents,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            new ReadOnlyDictionary<HomogeneousReference, decimal>(
                constituents.GroupBy(x => x.constituent.Id)
                .ToDictionary(x => x.First().constituent, x => x.Sum(y => y.proportion / constituents.Sum(z => z.proportion)))),
            id,
            name,
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    private Mixture(
        IReadOnlyDictionary<HomogeneousReference, decimal> constituents,
        string id,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            id,
            constituents,
            name ?? GetName(constituents),
            densityLiquid,
            densitySolid,
            densitySpecial)
    { }

    private Mixture(
        IReadOnlyDictionary<HomogeneousReference, decimal> constituents,
        string? name = null,
        double? densityLiquid = null,
        double? densitySolid = null,
        double? densitySpecial = null) : this(
            Guid.NewGuid().ToString(),
            constituents,
            name ?? GetName(constituents),
            densityLiquid,
            densitySolid,
            densitySpecial)
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
        var p = proportion.Clamp(0, 1);
        var ratio = included ? 1 - (p - match!.Value.Value) : 1 - p;
        var constuituents = new List<(HomogeneousReference, decimal)>();
        foreach (var keyValuePair in Constituents.ToList())
        {
            if (included && keyValuePair.Key.Equals(match!.Value.Key))
            {
                constuituents.Add((keyValuePair.Key, p));
            }
            else
            {
                constuituents.Add((keyValuePair.Key, keyValuePair.Value * ratio));
            }
        }
        if (!included)
        {
            constuituents.Add((constituent, p));
        }
        return new Mixture(constuituents);
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
        var included = Constituents.Any(x => x.Key.Equals(constituent));
        var match = included ? Constituents.First(x => x.Key.Equals(constituent)) : (KeyValuePair<HomogeneousReference, decimal>?)null;
        var p = proportion.Clamp(0, 1);
        var ratio = included ? 1 - (p - match!.Value.Value) : 1 - p;
        var constuituents = new List<(HomogeneousReference, decimal)>();
        foreach (var keyValuePair in Constituents.ToList())
        {
            if (included && keyValuePair.Key.Equals(match!.Value.Key))
            {
                constuituents.Add((keyValuePair.Key, p));
            }
            else
            {
                constuituents.Add((keyValuePair.Key, keyValuePair.Value * ratio));
            }
        }
        if (!included)
        {
            constuituents.Add((constituent.GetHomogeneousReference(), p));
        }
        return new Mixture(constuituents);
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

        if (s is Mixture other)
        {
            var allConstituents = new Dictionary<HomogeneousReference, decimal>();
            var ratio = 1 - proportion;
            foreach (var constituent in Constituents)
            {
                allConstituents.Add(constituent.Key, constituent.Value * ratio);
            }
            foreach (var otherConstituent in other.Constituents)
            {
                if (allConstituents.ContainsKey(otherConstituent.Key))
                {
                    allConstituents[otherConstituent.Key] += otherConstituent.Value * proportion;
                }
                else
                {
                    allConstituents.Add(otherConstituent.Key, otherConstituent.Value * proportion);
                }
            }
            return new Mixture(allConstituents.Select(x => (x.Key, x.Value)));
        }
        else if (s is IHomogeneous homogeneous)
        {
            var allConstituents = new Dictionary<HomogeneousReference, decimal>();
            var ratio = 1 - proportion;
            foreach (var constituent in Constituents)
            {
                allConstituents.Add(constituent.Key, constituent.Value * ratio);
            }
            foreach (var otherConstituent in homogeneous.Constituents)
            {
                if (allConstituents.ContainsKey(otherConstituent.Key))
                {
                    allConstituents[otherConstituent.Key] += otherConstituent.Value * proportion;
                }
                else
                {
                    allConstituents.Add(otherConstituent.Key, otherConstituent.Value * proportion);
                }
            }
            return new Mixture(allConstituents.Select(x => (x.Key, x.Value)));
        }
        else
        {
            return s.Combine(this, proportion);
        }
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

        if (substance is Mixture other)
        {
            var allConstituents = new Dictionary<HomogeneousReference, decimal>();
            var ratio = 1 - proportion;
            foreach (var constituent in Constituents)
            {
                allConstituents.Add(constituent.Key, constituent.Value * ratio);
            }
            foreach (var otherConstituent in other.Constituents)
            {
                if (allConstituents.ContainsKey(otherConstituent.Key))
                {
                    allConstituents[otherConstituent.Key] += otherConstituent.Value * proportion;
                }
                else
                {
                    allConstituents.Add(otherConstituent.Key, otherConstituent.Value * proportion);
                }
            }
            return new Mixture(allConstituents.Select(x => (x.Key, x.Value)));
        }
        else if (substance is IHomogeneous homogeneous)
        {
            var allConstituents = new Dictionary<HomogeneousReference, decimal>();
            var ratio = 1 - proportion;
            foreach (var constituent in Constituents)
            {
                allConstituents.Add(constituent.Key, constituent.Value * ratio);
            }
            foreach (var otherConstituent in homogeneous.Constituents)
            {
                if (allConstituents.ContainsKey(otherConstituent.Key))
                {
                    allConstituents[otherConstituent.Key] += otherConstituent.Value * proportion;
                }
                else
                {
                    allConstituents.Add(otherConstituent.Key, otherConstituent.Value * proportion);
                }
            }
            return new Mixture(allConstituents.Select(x => (x.Key, x.Value)));
        }
        else
        {
            return substance.Combine(this, proportion);
        }
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

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Mixture? other) => other is not null && Id.Equals(other.Id, StringComparison.Ordinal);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(IIdItem? other)
        => other is Mixture mixture && Equals(mixture);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ISubstance? other)
        => other is Mixture mixture && Equals(mixture);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ISubstanceReference? other)
        => other?.Equals(this) ?? false;

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
        if (DensitySolid.HasValue || DensityLiquid.HasValue)
        {
            var separation = SeparateByPhase(temperature, pressure, PhaseType.Solid, PhaseType.Liquid, PhaseType.Gas).ToList();
            if (DensitySolid.HasValue
                && separation[0].proportion >= separation[1].proportion
                && separation[0].proportion >= separation[2].proportion
                && separation[0].proportion >= separation[3].proportion)
            {
                return DensitySolid.Value;
            }
            if (DensityLiquid.HasValue
                && separation[1].proportion >= separation[0].proportion
                && separation[1].proportion >= separation[2].proportion
                && separation[1].proportion >= separation[3].proportion)
            {
                return DensityLiquid.Value;
            }
            if (DensitySpecial.HasValue
                && separation[0].proportion < separation[3].proportion
                && separation[1].proportion < separation[3].proportion
                && separation[2].proportion < separation[3].proportion)
            {
                return DensitySpecial.Value;
            }
        }
        return Constituents.Sum(x => x.Key.Homogeneous.GetDensity(temperature, pressure) * (double)x.Value);
    }

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// <para>
    /// In heterogeneous composites, gets a homogenized version of the mixture.
    /// </para>
    /// <para>
    /// For a <see cref="Mixture"/>, gets a <see cref="Solution"/> with the same constituents.
    /// </para>
    /// </summary>
    /// <returns>A homogenized version of a heterogeneous composites, or the substance
    /// itself.</returns>
    public ISubstance GetHomogenized() => new Solution(Constituents.Select(x => (x.Key, x.Value)));

    /// <summary>
    /// Gets the proportion of the given <paramref name="constituent"/> in this substance.
    /// </summary>
    /// <param name="constituent">An <see cref="IHomogeneous"/> constituent whose proportion in
    /// this instance will be determined.</param>
    /// <returns>The proportion of the given <paramref name="constituent"/> in this substance;
    /// or zero, if it does not contain the given <paramref name="constituent"/>.</returns>
    public decimal GetProportion(HomogeneousReference constituent)
    {
        if (Constituents is null)
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
        if (Constituents is null)
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
        => Constituents?.Sum(x => condition.Invoke(x.Key.Homogeneous) ? x.Value : 0) ?? 0;

    /// <summary>
    /// Gets an <see cref="ISubstanceReference"/> for this <see cref="Mixture"/>.
    /// </summary>
    /// <returns>An <see cref="ISubstanceReference"/> for this <see
    /// cref="Mixture"/>.</returns>
    public ISubstanceReference GetReference() => new SubstanceReference(this);

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
        var newConstituents = new Dictionary<HomogeneousReference, decimal>();
        foreach (var keyValuePair in Constituents)
        {
            if (keyValuePair.Key.Homogeneous is Chemical chemicalConstituent)
            {
                if (!chemicalConstituent.Equals(constituent))
                {
                    if (newConstituents.ContainsKey(keyValuePair.Key))
                    {
                        newConstituents[keyValuePair.Key] += keyValuePair.Value;
                    }
                    else
                    {
                        newConstituents.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }
            }
            else
            {
                var result = keyValuePair.Key.Homogeneous.Remove(constituent);
                if (!result.IsEmpty && result is IHomogeneous homogeneousResult)
                {
                    var newKey = homogeneousResult.GetHomogeneousReference();
                    if (newConstituents.ContainsKey(newKey))
                    {
                        newConstituents[newKey] += keyValuePair.Value;
                    }
                    else
                    {
                        newConstituents.Add(newKey, keyValuePair.Value);
                    }
                }
            }
        }
        var ratio = 1 / newConstituents.Sum(x => x.Value);
        if (ratio != 1)
        {
            foreach (var newConstituent in newConstituents.ToList())
            {
                newConstituents[newConstituent.Key] = newConstituent.Value * ratio;
            }
        }
        return new Mixture(newConstituents.Select(x => (x.Key, x.Value)));
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
        var newConstituents = new Dictionary<HomogeneousReference, decimal>();
        foreach (var keyValuePair in Constituents)
        {
            if (keyValuePair.Key.Homogeneous is Chemical chemicalConstituent)
            {
                if (!chemicalConstituent.Equals(constituent))
                {
                    if (newConstituents.ContainsKey(keyValuePair.Key))
                    {
                        newConstituents[keyValuePair.Key] += keyValuePair.Value;
                    }
                    else
                    {
                        newConstituents.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }
            }
            else
            {
                var result = keyValuePair.Key.Homogeneous.Remove(constituent);
                if (!result.IsEmpty && result is IHomogeneous homogeneousResult)
                {
                    var newKey = homogeneousResult.GetHomogeneousReference();
                    if (newConstituents.ContainsKey(newKey))
                    {
                        newConstituents[newKey] += keyValuePair.Value;
                    }
                    else
                    {
                        newConstituents.Add(newKey, keyValuePair.Value);
                    }
                }
            }
        }
        var ratio = 1 / newConstituents.Sum(x => x.Value);
        if (ratio != 1)
        {
            foreach (var newConstituent in newConstituents.ToList())
            {
                newConstituents[newConstituent.Key] = newConstituent.Value * ratio;
            }
        }
        return new Mixture(newConstituents.Select(x => (x.Key, x.Value)));
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
    public ISubstance WithSubstanceName(string name) => new Mixture(Constituents, name, DensityLiquid, DensitySolid, DensitySpecial);

    private static string GetName(IReadOnlyDictionary<HomogeneousReference, decimal> constituents)
    {
        var sb = new StringBuilder();
        foreach (var constituent in constituents)
        {
            if (sb.Length > 0)
            {
                sb.Append("; ");
            }
            sb.Append(constituent.Key.Homogeneous.Name)
                .Append(':')
                .Append(constituent.Value.ToString("P3"));
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
    public static bool operator ==(Mixture left, ISubstance right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two substances are unequal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are unequal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(Mixture left, ISubstance right) => !(left == right);
}
