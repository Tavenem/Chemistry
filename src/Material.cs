using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Json.Serialization;
using Tavenem.Mathematics;

namespace Tavenem.Chemistry;

/// <summary>
/// A physical object with a size and shape, temperature, and overall density. It may or may not
/// also have a particular chemical composition.
/// </summary>
public class Material<TScalar> : IMaterial<Material<TScalar>, TScalar>, IEquatable<Material<TScalar>>
    where TScalar : IFloatingPointIeee754<TScalar>
{
    /// <summary>
    /// An empty material, with zero mass and density, and a single point as a shape.
    /// </summary>
    public static Material<TScalar> Empty { get; } = new();

    /// <summary>
    /// This material's constituent substances.
    /// </summary>
    [JsonConverter(typeof(MixtureConstituentsConverter))]
    public IReadOnlyDictionary<ISubstanceReference, decimal> Constituents { get; private set; }

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
    public double Density { get; set; }

    /// <summary>
    /// Whether this material is an empty instance.
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty =>
        Shape.Equals(SinglePoint<TScalar>.Origin)
        && Constituents.Count == 0
        && !Temperature.HasValue
        && Density == 0
        && Mass == TScalar.Zero;

    /// <summary>
    /// The mass of this material, in kg.
    /// </summary>
    public TScalar Mass { get; set; }

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
    [JsonIgnore]
    public Vector3<TScalar> Position
    {
        get => Shape.Position;
        set => Shape = Shape.GetCloneAtPosition(value);
    }

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
    [JsonIgnore]
    public Quaternion<TScalar> Rotation
    {
        get => Shape.Rotation;
        set => Shape = Shape.GetCloneWithRotation(value);
    }

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
    [JsonConverter(typeof(ShapeConverterFactory))]
    public IShape<TScalar> Shape { get; set; }

    /// <summary>
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// Initializes a new, empty instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    public Material()
    {
        Density = 0;
        Mass = TScalar.Zero;
        Shape = SinglePoint<TScalar>.Origin;
        Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(new Dictionary<ISubstanceReference, decimal>());
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="constituents">
    /// This material's constituents.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="mass">The mass of this material, in kg.</param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IReadOnlyDictionary<ISubstanceReference, decimal> constituents,
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null)
    {
        Constituents = constituents;
        Shape = shape;
        Mass = mass;
        Density = density ?? (mass / shape.Volume).CreateChecked<TScalar, double>();
        Temperature = temperature;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="constituents">
    /// This material's constituents.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IReadOnlyDictionary<ISubstanceReference, decimal> constituents,
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null)
    {
        Constituents = constituents;
        Shape = shape;
        Density = density
            ?? Constituents.Sum(x =>
                x.Key.Substance.GetDensity(temperature ?? 273, 101.325)
                * (double)x.Value);
        Mass = Density == 0
            ? TScalar.Zero
            : TScalar.CreateChecked(Density) * shape.Volume;
        Temperature = temperature;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="substance">
    /// A substance which comprise this material's only constituent.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="mass">The mass of this material, in kg.</param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        ISubstanceReference substance,
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null) : this(
            new ReadOnlyDictionary<ISubstanceReference, decimal>(
                new Dictionary<ISubstanceReference, decimal> { { substance, 1 } }),
            shape,
            mass,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="substance">
    /// A substance which comprise this material's only constituent.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        ISubstanceReference substance,
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null) : this(
            new ReadOnlyDictionary<ISubstanceReference, decimal>(
                new Dictionary<ISubstanceReference, decimal> { { substance, 1 } }),
            shape,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="substance">
    /// A substance which comprise this material's only constituent.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="mass">The mass of this material, in kg.</param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        ISubstance substance,
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null)
        : this(substance.GetReference(), shape, mass, density, temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="substance">
    /// A substance which comprise this material's only constituent.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        ISubstance substance,
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null)
        : this(substance.GetReference(), shape, density, temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="mass">The mass of this material, in kg.</param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null) : this(
            new ReadOnlyDictionary<ISubstanceReference, decimal>(
                new Dictionary<ISubstanceReference, decimal>()),
            shape,
            mass,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> the material will have zero
    /// mass and density.
    /// </remarks>
    public Material(
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null) : this(
            new ReadOnlyDictionary<ISubstanceReference, decimal>(
                new Dictionary<ISubstanceReference, decimal>()),
            shape,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="substances">
    /// The substances which comprise this material's constituents.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="mass">The mass of this material, in kg.</param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IEnumerable<(ISubstanceReference substance, decimal proportion)>? substances,
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null)
    {
        Shape = shape;
        if (substances is not null)
        {
            var substanceList = substances.ToList();
            var total = substanceList.Sum(x => x.proportion);
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(
                substances.ToDictionary(
                    x => x.substance,
                    x => total == 1
                        ? x.proportion
                        : x.proportion / total));
        }
        else
        {
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(new Dictionary<ISubstanceReference, decimal>());
        }
        Mass = mass;
        Density = density ?? (mass / shape.Volume).CreateChecked<TScalar, double>();
        Temperature = temperature;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="substances">
    /// The substances which comprise this material's constituents.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IEnumerable<(ISubstanceReference substance, decimal proportion)>? substances,
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null)
    {
        Shape = shape;
        if (substances is not null)
        {
            var substanceList = substances.ToList();
            var total = substanceList.Sum(x => x.proportion);
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(
                substances.ToDictionary(
                    x => x.substance,
                    x => total == 1
                        ? x.proportion
                        : x.proportion / total));
        }
        else
        {
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(new Dictionary<ISubstanceReference, decimal>());
        }
        Density = density
            ?? Constituents.Sum(x =>
                x.Key.Substance.GetDensity(temperature ?? 273, 101.325)
                * (double)x.Value);
        Mass = Density == 0
            ? TScalar.Zero
            : TScalar.CreateChecked(Density) * shape.Volume;
        Temperature = temperature;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="substances">
    /// The substances which comprise this material's constituents.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="mass">The mass of this material, in kg.</param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IEnumerable<(ISubstance substance, decimal proportion)>? substances,
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null)
        : this(
            substances?.Select(x => (x.substance.GetReference(), x.proportion)),
            shape,
            mass,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="substances">
    /// The substances which comprise this material's constituents.
    /// </param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IEnumerable<(ISubstance substance, decimal proportion)>? substances,
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null)
        : this(
            substances?.Select(x => (x.substance.GetReference(), x.proportion)),
            shape,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="mass">The mass of this material, in kg.</param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <param name="substances">
    /// The substances which comprise this material's constituents.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null,
        params (ISubstanceReference substance, decimal proportion)[] substances) : this(
            substances.AsEnumerable(),
            shape,
            mass,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <param name="substances">
    /// The substances which comprise this material's constituents.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null,
        params (ISubstanceReference substance, decimal proportion)[] substances) : this(
            substances.AsEnumerable(),
            shape,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="mass">The mass of this material, in kg.</param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <param name="substances">
    /// The substances which comprise this material's constituents.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null,
        params (ISubstance substance, decimal proportion)[] substances) : this(
            substances.Select(x => (x.substance.GetReference(), x.proportion)),
            shape,
            mass,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <param name="substances">
    /// The substances which comprise this material's constituents.
    /// </param>
    /// <remarks>
    /// If <paramref name="density"/> is left <see langword="null"/> it will be calculated based on
    /// the shape and properties of the constituents.
    /// </remarks>
    public Material(
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null,
        params (ISubstance substance, decimal proportion)[] substances) : this(
            substances.Select(x => (x.substance.GetReference(), x.proportion)),
            shape,
            density,
            temperature)
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="Material{TScalar}"/>.
    /// </summary>
    /// <param name="constituents">
    /// This material's constituents.
    /// </param>
    /// <param name="density">
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// </param>
    /// <param name="mass">The mass of this material, in kg.</param>
    /// <param name="shape">
    /// <para>
    /// The overall shape of this material.
    /// </para>
    /// <para>
    /// A material may be fully distributed throughout its indicated shape, or its shape may
    /// represent an approximation which contains an irregular, actual shape to some degree of
    /// approximation.
    /// </para>
    /// </param>
    /// <param name="temperature">
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </param>
    /// <remarks>
    /// If either <paramref name="mass"/> or <paramref name="density"/> are left <see
    /// langword="null"/> they will be calculated based on the shape and properties of the
    /// constituents.
    /// </remarks>
    [JsonConstructor]
    public Material(
        IReadOnlyDictionary<ISubstanceReference, decimal> constituents,
        double density,
        TScalar mass,
        IShape<TScalar> shape,
        double? temperature)
    {
        Constituents = constituents;
        Density = density;
        Mass = mass;
        Shape = shape;
        Temperature = temperature;
    }

    /// <summary>
    /// Adds the given <paramref name="substance"/> as a new constituent of this material, in the
    /// given <paramref name="proportion"/>.
    /// </summary>
    /// <param name="substance">
    /// The new substance to add. If the given substance already exists in this material's
    /// conposition, its proportion is adjusted to the given value.
    /// </param>
    /// <param name="proportion">
    /// The proportion at which to add the <paramref name="substance"/>.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(ISubstanceReference substance, decimal proportion = 0.5m)
        => AddConstituent(substance, proportion);

    /// <summary>
    /// Adds the given <paramref name="substance"/> as a new constituent of this material, in the
    /// given <paramref name="proportion"/>.
    /// </summary>
    /// <param name="substance">
    /// The new substance to add. If the given substance already exists in this material's
    /// conposition, its proportion is adjusted to the given value.
    /// </param>
    /// <param name="proportion">
    /// The proportion at which to add the <paramref name="substance"/>.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(ISubstance substance, decimal proportion = 0.5m)
        => AddConstituent(substance.GetReference(), proportion);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's conposition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(IEnumerable<(ISubstanceReference substance, decimal proportion)> constituents)
        => AddConstituents(constituents);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's conposition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(IEnumerable<(ISubstance substance, decimal proportion)> constituents)
        => AddConstituents(constituents);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's conposition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(params (ISubstanceReference substance, decimal proportion)[] constituents)
        => AddConstituents(constituents.AsEnumerable());

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's conposition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(params (ISubstance substance, decimal proportion)[] constituents)
        => AddConstituents(constituents.AsEnumerable());

    /// <summary>
    /// Adds the given <paramref name="substance"/> as a new constituent of this material, in the
    /// given <paramref name="proportion"/>.
    /// </summary>
    /// <param name="substance">
    /// The new substance to add. If the given substance already exists in this material's
    /// conposition, its proportion is adjusted to the given value.
    /// </param>
    /// <param name="proportion">
    /// The proportion at which to add the <paramref name="substance"/>.
    /// </param>
    /// <returns>This instance.</returns>
    public Material<TScalar> AddConstituent(ISubstanceReference substance, decimal proportion = 0.5m)
    {
        if (substance is null || proportion <= 0)
        {
            return this;
        }
        if (proportion >= 1 || Constituents.Count == 0)
        {
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(
                new Dictionary<ISubstanceReference, decimal>(
                    new[] { new KeyValuePair<ISubstanceReference, decimal>(substance, 1) }));
            return this;
        }

        var dictionary = Constituents.ToDictionary(x => x.Key, x => x.Value);
        var ratio = dictionary.TryGetValue(substance, out var value)
            ? 1 - (proportion - value)
            : 1 - proportion;
        foreach (var key in dictionary.Keys)
        {
            dictionary[key] *= ratio;
        }
        dictionary[substance] = proportion;

        Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(dictionary);
        return this;
    }

    /// <summary>
    /// Adds the given <paramref name="substance"/> as a new constituent of this material, in the
    /// given <paramref name="proportion"/>.
    /// </summary>
    /// <param name="substance">
    /// The new substance to add. If the given substance already exists in this material's
    /// conposition, its proportion is adjusted to the given value.
    /// </param>
    /// <param name="proportion">
    /// The proportion at which to add the <paramref name="substance"/>.
    /// </param>
    /// <returns>This instance.</returns>
    public Material<TScalar> AddConstituent(ISubstance substance, decimal proportion = 0.5m)
        => AddConstituent(substance.GetReference(), proportion);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">The new constituents to add, as a tuple of a substance and
    /// the proportion to assign to that substance. If a given substance already exists in
    /// this material's conposition, its proportion is adjusted to the given value.</param>
    /// <returns>This instance.</returns>
    public Material<TScalar> AddConstituents(IEnumerable<(ISubstanceReference substance, decimal proportion)> constituents)
    {
        var constituentList = constituents.ToList();
        if (constituentList.Count == 0)
        {
            return this;
        }
        var addedProportion = constituentList.Sum(x => x.proportion);
        if (addedProportion <= 0)
        {
            return this;
        }
        if (addedProportion >= 1 || Constituents.Count == 0)
        {
            var apRatio = 1 / addedProportion;
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(
                new Dictionary<ISubstanceReference, decimal>(
                    constituentList.Select(x => new KeyValuePair<ISubstanceReference, decimal>(x.substance, x.proportion * apRatio))));
            return this;
        }

        var dictionary = Constituents.ToDictionary(x => x.Key, x => x.Value);
        var ratio = 1 - addedProportion;
        foreach (var key in dictionary.Keys)
        {
            dictionary[key] *= ratio;
        }

        foreach (var (substance, proportion) in constituentList)
        {
            dictionary[substance] = proportion;
        }

        Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(dictionary);
        return this;
    }

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">The new constituents to add, as a tuple of a substance and
    /// the proportion to assign to that substance. If a given substance already exists in
    /// this material's conposition, its proportion is adjusted to the given value.</param>
    /// <returns>This instance.</returns>
    public Material<TScalar> AddConstituents(IEnumerable<(ISubstance substance, decimal proportion)> constituents)
        => AddConstituents(constituents.Select(x => (x.substance.GetReference(), x.proportion)));

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">The new constituents to add, as a tuple of a substance and
    /// the proportion to assign to that substance. If a given substance already exists in
    /// this material's conposition, its proportion is adjusted to the given value.</param>
    /// <returns>This instance.</returns>
    public Material<TScalar> AddConstituents(params (ISubstanceReference substance, decimal proportion)[] constituents)
        => AddConstituents(constituents.AsEnumerable());

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">The new constituents to add, as a tuple of a substance and
    /// the proportion to assign to that substance. If a given substance already exists in
    /// this material's conposition, its proportion is adjusted to the given value.</param>
    /// <returns>This instance.</returns>
    public Material<TScalar> AddConstituents(params (ISubstance substance, decimal proportion)[] constituents)
        => AddConstituents(constituents.Select(x => (x.substance.GetReference(), x.proportion)));

    /// <summary>Creates a new object that is a copy of the current instance.</summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    /// <remarks>
    /// See <see cref="GetClone()"/> or <see cref="GetTypedClone()"/> for a strongly typed version
    /// of this method.
    /// </remarks>
    public object Clone() => GetTypedClone();

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Material<TScalar>? other)
        => other is not null
        && Density == other.Density
        && Mass == other.Mass
        && Shape.Equals(other.Shape)
        && Constituents.OrderBy(x => x.Key).SequenceEqual(other.Constituents.OrderBy(y => y.Key))
        && EqualityComparer<double?>.Default.Equals(Temperature, other.Temperature);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="other">obj</paramref> and this instance
    /// are the same type and represent the same value; otherwise, <see
    /// langword="false"/>.</returns>
    public bool Equals(IMaterial<TScalar>? other) => other is Material<TScalar> material && Equals(material);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance
    /// are the same type and represent the same value; otherwise, <see
    /// langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Material<TScalar> other && Equals(other);

    /// <summary>
    /// Gets a deep clone of this <see cref="IMaterial{TScalar}"/> instance, possibly with a different
    /// mass.
    /// </summary>
    /// <param name="massFraction">
    /// <para>
    /// The proportion of this instance's mass to assign to the clone.
    /// </para>
    /// <para>
    /// Values ≤ 0 result in <see cref="Material{TScalar}.Empty"/> being returned.
    /// </para>
    /// </param>
    /// <returns>A deep clone of this instance, possibly with a different mass.</returns>
    public IMaterial<TScalar> GetClone(TScalar massFraction) => GetTypedClone(massFraction);

    /// <summary>
    /// Gets a deep clone of this <see cref="IMaterial{TScalar}"/> instance.
    /// </summary>
    /// <returns>A deep clone of this instance.</returns>
    public IMaterial<TScalar> GetClone() => GetTypedClone();

    /// <summary>
    /// <para>
    /// In composites, gets the first layer.
    /// </para>
    /// <para>
    /// In other materials, gets the material itself.
    /// </para>
    /// </summary>
    /// <returns>The first layer of a composite, or the material itself.</returns>
    public IMaterial<TScalar> GetCore() => this;

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
    public IMaterial<TScalar> GetHomogenized() => this;

    /// <summary>
    /// <para>
    /// In composites, gets the last layer.
    /// </para>
    /// <para>
    /// In other materials, gets the material itself.
    /// </para>
    /// </summary>
    /// <returns>The last layer of a composite, or the material itself.</returns>
    public IMaterial<TScalar> GetSurface() => this;

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(Constituents, Shape, Mass, Density, Temperature);

    /// <summary>
    /// Gets a deep clone of this <see cref="IMaterial{TSelf, TScalar}"/> instance, possibly with a
    /// different mass.
    /// </summary>
    /// <param name="massFraction">
    /// <para>
    /// The proportion of this instance's mass to assign to the clone.
    /// </para>
    /// <para>
    /// Values ≤ 0 result in <see cref="Material{TScalar}.Empty"/> being returned.
    /// </para>
    /// </param>
    /// <returns>A deep clone of this instance, possibly with a different mass.</returns>
    public Material<TScalar> GetTypedClone(TScalar massFraction)
        => massFraction <= TScalar.Zero
        ? Empty
        : new Material<TScalar>(
            new ReadOnlyDictionary<ISubstanceReference, decimal>(Constituents.ToDictionary(x => x.Key, x => x.Value)),
            Density,
            massFraction != TScalar.One
                ? Mass * massFraction
                : Mass,
            Shape,
            Temperature);

    /// <summary>
    /// Gets a deep clone of this <see cref="IMaterial{TSelf, TScalar}"/> instance.
    /// </summary>
    /// <returns>A deep clone of this instance.</returns>
    public Material<TScalar> GetTypedClone() => new(
        new ReadOnlyDictionary<ISubstanceReference, decimal>(Constituents.ToDictionary(x => x.Key, x => x.Value)),
        Density,
        Mass,
        Shape,
        Temperature);

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
    public IMaterial<TScalar> Remove(Predicate<ISubstance> match) => RemoveConstituents(match);

    /// <summary>
    /// <para>
    /// Removes the given substance from this material.
    /// </para>
    /// <para>
    /// Has no effect if the substance is not present.
    /// </para>
    /// </summary>
    /// <param name="substance">The substance to remove.</param>
    /// <returns>This instance.</returns>
    /// <remarks>
    /// Removes all copies of the substance, if more than one version happens to be present.
    /// </remarks>
    public Material<TScalar> RemoveConstituent(ISubstanceReference substance)
    {
        if (!Constituents.TryGetValue(substance, out var proportion))
        {
            return this;
        }

        if (Constituents.Count == 1)
        {
            Density = 0;
            Mass = TScalar.Zero;
            Shape = SinglePoint<TScalar>.Origin;
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(new Dictionary<ISubstanceReference, decimal>());
            Temperature = null;
            return Empty;
        }

        var dictionary = Constituents.ToDictionary(x => x.Key, x => x.Value);
        var ratio = proportion == 0 ? 1 : 1 / (1 - proportion);

        dictionary.Remove(substance);

        foreach (var key in dictionary.Keys)
        {
            dictionary[key] *= ratio;
        }

        Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(dictionary);
        return this;
    }

    /// <summary>
    /// <para>
    /// Removes the given substance from this material.
    /// </para>
    /// <para>
    /// Has no effect if the substance is not present.
    /// </para>
    /// </summary>
    /// <param name="substance">The substance to remove.</param>
    /// <returns>This instance.</returns>
    /// <remarks>
    /// Removes all copies of the substance, if more than one version happens to be present.
    /// </remarks>
    public Material<TScalar> RemoveConstituent(ISubstance substance)
    {
        if (!Constituents.TryGetValue(substance.GetReference(), out var proportion))
        {
            return this;
        }

        if (Constituents.Count == 1)
        {
            Density = 0;
            Mass = TScalar.Zero;
            Shape = SinglePoint<TScalar>.Origin;
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(new Dictionary<ISubstanceReference, decimal>());
            Temperature = null;
            return Empty;
        }

        var dictionary = Constituents.ToDictionary(x => x.Key, x => x.Value);
        var ratio = proportion == 0 ? 1 : 1 / (1 - proportion);

        dictionary.Remove(substance.GetReference());

        foreach (var key in dictionary.Keys)
        {
            dictionary[key] *= ratio;
        }

        Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(dictionary);
        return this;
    }

    /// <summary>
    /// <para>
    /// Removes all substances which satisfy the given condition from this material.
    /// </para>
    /// <para>
    /// Has no effect if the substance is not present.
    /// </para>
    /// </summary>
    /// <param name="match">The <see cref="Predicate{T}"/> that defines the conditions of the
    /// substances to remove.</param>
    /// <returns>This instance.</returns>
    public Material<TScalar> RemoveConstituents(Predicate<ISubstance> match)
    {
        var dictionary = new Dictionary<ISubstanceReference, decimal>();

        var total = 0m;
        foreach (var (key, value) in Constituents)
        {
            if (!match.Invoke(key.Substance))
            {
                dictionary.Add(key, value);
                total += value;
            }
        }

        if (dictionary.Count == 0 || total == 0)
        {
            Density = 0;
            Mass = TScalar.Zero;
            Shape = SinglePoint<TScalar>.Origin;
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(new Dictionary<ISubstanceReference, decimal>());
            Temperature = null;
            return Empty;
        }

        var ratio = 1 / total;

        foreach (var key in dictionary.Keys)
        {
            dictionary[key] *= ratio;
        }

        Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(dictionary);
        return this;
    }

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
    /// <remarks>If the given <paramref name="proportions"/> do not sum to 1, they are
    /// normalized.</remarks>
    public IMaterial<TScalar> Split(params TScalar[] proportions)
    {
        if (proportions.Length == 1
            && (proportions[0] <= TScalar.Zero
            || proportions[0] >= TScalar.One))
        {
            return this;
        }
        if (proportions.Length == 0)
        {
            var half = NumberValues.Half<TScalar>();
            proportions = new TScalar[] { half, half };
        }
        else if (proportions.Length == 1)
        {
            proportions = new TScalar[] { proportions[0], TScalar.One - proportions[0] };
        }
        else
        {
            var sum = proportions.Sum();
            if (sum != TScalar.One)
            {
                for (var i = 0; i < proportions.Length; i++)
                {
                    proportions[i] /= sum;
                }
            }
        }
        var components = new List<IMaterial<TScalar>>();
        for (var i = 0; i < proportions.Length; i++)
        {
            components.Add(GetClone(proportions[i]));
        }
        return new Composite<TScalar>(components, Shape, null, null);
    }
}
