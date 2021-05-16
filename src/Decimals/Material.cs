using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Tavenem.Mathematics.Decimals;

namespace Tavenem.Chemistry.Decimals
{
    /// <summary>
    /// A physical object with a size and shape, temperature, and overall density. It may or may not
    /// also have a particular chemical composition.
    /// </summary>
    [Serializable]
    [DataContract]
    public class Material : IMaterial, ISerializable, IEquatable<Material>
    {
        /// <summary>
        /// An empty material, with zero mass and density, and a single point as a shape.
        /// </summary>
        public static Material Empty => new();

        /// <summary>
        /// This material's constituent substances.
        /// </summary>
        [DataMember(Order = 1)]
        [System.Text.Json.Serialization.JsonConverter(typeof(MixtureConstituentsConverter))]
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
        [DataMember(Order = 2)]
        public double Density { get; set; }

        /// <summary>
        /// Whether this material is an empty instance.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsEmpty =>
            Shape.Equals(SinglePoint.Origin)
            && Constituents.Count == 0
            && !Temperature.HasValue
            && Density == 0
            && Mass == 0;

        /// <summary>
        /// The mass of this material, in kg.
        /// </summary>
        [DataMember(Order = 3)]
        public decimal Mass { get; set; }

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
        [System.Text.Json.Serialization.JsonIgnore]
        public Vector3 Position
        {
            get => Shape.Position;
            set => Shape = Shape.GetCloneAtPosition(value);
        }

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
        [System.Text.Json.Serialization.JsonIgnore]
        public Quaternion Rotation
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
        [DataMember(Order = 4)]
        [System.Text.Json.Serialization.JsonConverter(typeof(ShapeConverter))]
        public IShape Shape { get; set; }

        /// <summary>
        /// The average temperature of this material, in K. May be <see langword="null"/>,
        /// indicating that it is at the ambient temperature of its environment.
        /// </summary>
        [DataMember(Order = 5)]
        public double? Temperature { get; set; }

        /// <summary>
        /// Initializes a new, empty instance of <see cref="Material"/>.
        /// </summary>
        public Material()
        {
            Density = 0;
            Mass = 0;
            Shape = SinglePoint.Origin;
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(new Dictionary<ISubstanceReference, decimal>());
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Material"/>.
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
        /// If either <paramref name="mass"/> or <paramref name="density"/> are left <see
        /// langword="null"/> they will be calculated based on the shape and properties of the
        /// constituents.
        /// </remarks>
        public Material(ISubstanceReference substance, IShape shape, decimal? mass = null, double? density = null, double? temperature = null)
        {
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(
                new Dictionary<ISubstanceReference, decimal> { { substance, 1 } });
            Shape = shape;
            if (!density.HasValue)
            {
                density = mass.HasValue
                    ? (double)(mass / shape.Volume)
                    : substance.Substance.GetDensity(temperature ?? 273, 101.325);
            }
            if (!mass.HasValue)
            {
                mass = (decimal)density * shape.Volume;
            }
            Mass = mass.Value;
            Density = density.Value;
            Temperature = temperature;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Material"/>.
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
        /// If either <paramref name="mass"/> or <paramref name="density"/> are left <see
        /// langword="null"/> they will be calculated based on the shape and properties of the
        /// constituents.
        /// </remarks>
        public Material(ISubstance substance, IShape shape, decimal? mass = null, double? density = null, double? temperature = null)
            : this(substance.GetReference(), shape, mass, density, temperature)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="Material"/>.
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
        /// <para>
        /// If either <paramref name="mass"/> or <paramref name="density"/> are left <see
        /// langword="null"/> they will be calculated based on the shape and properties of the
        /// constituents.
        /// </para>
        /// <para>
        /// If both are <see langword="null"/> the material will have zero mass and density.
        /// </para>
        /// </remarks>
        public Material(IShape shape, decimal? mass = null, double? density = null, double? temperature = null)
        {
            Shape = shape;
            if (!density.HasValue)
            {
                density = mass.HasValue
                    ? (double)(mass / shape.Volume)
                    : 0;
            }
            if (!mass.HasValue)
            {
                mass = (decimal)density * shape.Volume;
            }
            Density = density.Value;
            Mass = mass.Value;
            Temperature = temperature;
            Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(
                new Dictionary<ISubstanceReference, decimal>());
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Material"/>.
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
        /// If either <paramref name="mass"/> or <paramref name="density"/> are left <see
        /// langword="null"/> they will be calculated based on the shape and properties of the
        /// constituents.
        /// </remarks>
        public Material(IEnumerable<(ISubstanceReference substance, decimal proportion)>? substances, IShape shape, decimal? mass = null, double? density = null, double? temperature = null)
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
            if (!density.HasValue)
            {
                density = mass.HasValue
                    ? (double)(mass / shape.Volume)
                    : Constituents.Sum(x => x.Key.Substance.GetDensity(temperature ?? 273, 101.325) * (double)x.Value);
            }
            if (!mass.HasValue)
            {
                mass = (decimal)density * shape.Volume;
            }
            Mass = mass.Value;
            Density = density.Value;
            Temperature = temperature;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Material"/>.
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
        /// If either <paramref name="mass"/> or <paramref name="density"/> are left <see
        /// langword="null"/> they will be calculated based on the shape and properties of the
        /// constituents.
        /// </remarks>
        public Material(IEnumerable<(ISubstance substance, decimal proportion)>? substances, IShape shape, decimal? mass = null, double? density = null, double? temperature = null)
            : this(substances?.Select(x => (x.substance.GetReference(), x.proportion)), shape, mass, density, temperature)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="Material"/>.
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
        /// If either <paramref name="mass"/> or <paramref name="density"/> are left <see
        /// langword="null"/> they will be calculated based on the shape and properties of the
        /// constituents.
        /// </remarks>
        public Material(IShape shape, decimal? mass = null, double? density = null, double? temperature = null, params (ISubstanceReference substance, decimal proportion)[] substances)
        {
            Shape = shape;
            if (substances.Length > 0)
            {
                var total = substances.Sum(x => x.proportion);
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
            if (!density.HasValue)
            {
                density = mass.HasValue
                    ? (double)(mass / shape.Volume)
                    : Constituents.Sum(x => x.Key.Substance.GetDensity(temperature ?? 273, 101.325) * (double)x.Value);
            }
            if (!mass.HasValue)
            {
                mass = (decimal)density * shape.Volume;
            }
            Mass = mass.Value;
            Density = density.Value;
            Temperature = temperature;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Material"/>.
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
        /// If either <paramref name="mass"/> or <paramref name="density"/> are left <see
        /// langword="null"/> they will be calculated based on the shape and properties of the
        /// constituents.
        /// </remarks>
        public Material(IShape shape, decimal? mass = null, double? density = null, double? temperature = null, params (ISubstance substance, decimal proportion)[] substances)
            : this(shape, mass, density, temperature, substances.Select(x => (x.substance.GetReference(), x.proportion)).ToArray())
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="Material"/>.
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
        /// If either <paramref name="mass"/> or <paramref name="density"/> are left <see
        /// langword="null"/> they will be calculated based on the shape and properties of the
        /// constituents.
        /// </remarks>
        public Material(IDictionary<ISubstanceReference, decimal>? substances, IShape shape, decimal? mass = null, double? density = null, double? temperature = null)
        {
            if (substances is null
                || substances.Count == 0)
            {
                Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(new Dictionary<ISubstanceReference, decimal>());
            }
            else if (substances.Count == 1)
            {
                Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(substances.ToDictionary(x => x.Key, x => 1m));
            }
            else
            {
                var total = substances.Values.Sum();
                Constituents = new ReadOnlyDictionary<ISubstanceReference, decimal>(
                    substances.ToDictionary(
                        x => x.Key,
                        x => total == 1
                            ? x.Value
                            : x.Value / total));
            }
            Shape = shape;
            if (!density.HasValue)
            {
                density = mass.HasValue
                    ? (double)(mass / shape.Volume)
                    : Constituents.Sum(x => x.Key.Substance.GetDensity(temperature ?? 273, 101.325) * (double)x.Value);
            }
            if (!mass.HasValue)
            {
                mass = (decimal)density * shape.Volume;
            }
            Mass = mass.Value;
            Density = density.Value;
            Temperature = temperature;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Material"/>.
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
        [System.Text.Json.Serialization.JsonConstructor]
        public Material(IReadOnlyDictionary<ISubstanceReference, decimal> constituents, double density, decimal mass, IShape shape, double? temperature)
        {
            Constituents = constituents;
            Density = density;
            Mass = mass;
            Shape = shape;
            Temperature = temperature;
        }

        private Material(SerializationInfo info, StreamingContext context) : this(
            (IReadOnlyDictionary<ISubstanceReference, decimal>?)info.GetValue(nameof(Constituents), typeof(IReadOnlyDictionary<ISubstanceReference, decimal>)) ?? new ReadOnlyDictionary<ISubstanceReference, decimal>(new Dictionary<ISubstanceReference, decimal>()),
            (double?)info.GetValue(nameof(Density), typeof(double)) ?? default,
            (decimal?)info.GetValue(nameof(Mass), typeof(decimal)) ?? default,
            (IShape?)info.GetValue(nameof(Shape), typeof(IShape)) ?? SinglePoint.Origin,
            (double?)info.GetValue(nameof(Temperature), typeof(double?)))
        { }

        /// <summary>
        /// Adds a new constituent to this material, at the given <paramref name="proportion"/>.
        /// </summary>
        /// <param name="substance">The new substance to add. If the given substance already exists
        /// in this material's conposition, its proportion is adjusted to the given value.</param>
        /// <param name="proportion">The proportion at which to add the <paramref
        /// name="substance"/>.</param>
        /// <returns>This instance.</returns>
        public Material AddConstituent(ISubstanceReference substance, decimal proportion = 0.5m)
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
            var ratio = dictionary.ContainsKey(substance)
                ? 1 - (proportion - dictionary[substance])
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
        /// Adds a new constituent to this material, at the given <paramref name="proportion"/>.
        /// </summary>
        /// <param name="substance">The new substance to add. If the given substance already exists
        /// in this material's conposition, its proportion is adjusted to the given value.</param>
        /// <param name="proportion">The proportion at which to add the <paramref
        /// name="substance"/>.</param>
        /// <returns>This instance.</returns>
        public Material AddConstituent(ISubstance substance, decimal proportion = 0.5m)
            => AddConstituent(substance.GetReference(), proportion);

        /// <summary>
        /// Adds one or more new constituents to this material, at the given proportions.
        /// </summary>
        /// <param name="constituents">The new constituents to add, as a tuple of a substance and
        /// the proportion to assign to that substance. If a given substance already exists in
        /// this material's conposition, its proportion is adjusted to the given value.</param>
        /// <returns>This instance.</returns>
        public Material AddConstituents(IEnumerable<(ISubstanceReference substance, decimal proportion)> constituents)
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
        public Material AddConstituents(IEnumerable<(ISubstance substance, decimal proportion)> constituents)
            => AddConstituents(constituents.Select(x => (x.substance.GetReference(), x.proportion)));

        /// <summary>
        /// Adds one or more new constituents to this material, at the given proportions.
        /// </summary>
        /// <param name="constituents">The new constituents to add, as a tuple of a substance and
        /// the proportion to assign to that substance. If a given substance already exists in
        /// this material's conposition, its proportion is adjusted to the given value.</param>
        /// <returns>This instance.</returns>
        public Material AddConstituents(params (ISubstanceReference substance, decimal proportion)[] constituents)
            => AddConstituents(constituents.AsEnumerable());

        /// <summary>
        /// Adds one or more new constituents to this material, at the given proportions.
        /// </summary>
        /// <param name="constituents">The new constituents to add, as a tuple of a substance and
        /// the proportion to assign to that substance. If a given substance already exists in
        /// this material's conposition, its proportion is adjusted to the given value.</param>
        /// <returns>This instance.</returns>
        public Material AddConstituents(params (ISubstance substance, decimal proportion)[] constituents)
            => AddConstituents(constituents.Select(x => (x.substance.GetReference(), x.proportion)));

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        /// <remarks>See <see cref="GetClone(decimal)"/> for a strongly typed version of
        /// this method.</remarks>
        public object Clone() => GetClone(1);

        /// <summary>Indicates whether the current object is equal to another object of the same
        /// type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if the current object is equal to the <paramref
        /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Material? other)
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
        public bool Equals(IMaterial? other) => other is Material material && Equals(material);

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance
        /// are the same type and represent the same value; otherwise, <see
        /// langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is Material other && Equals(other);

        /// <summary>
        /// Gets a deep clone of this <see cref="IMaterial"/> instance, optionally at a different
        /// mass.
        /// </summary>
        /// <param name="massFraction">
        /// <para>
        /// The proportion of this instance's mass to assign to the clone.
        /// </para>
        /// <para>
        /// Values less than result in <see cref="Empty"/> being returned.
        /// </para>
        /// </param>
        /// <returns>A deep clone of this instance, optionally with a different mass.</returns>
        public IMaterial GetClone(decimal massFraction = 1)
            => massFraction <= 0
            ? Empty
            : new Material(
                new ReadOnlyDictionary<ISubstanceReference, decimal>(Constituents.ToDictionary(x => x.Key, x => x.Value)),
                Density,
                massFraction != 1 ? Mass * massFraction : Mass,
                Shape,
                Temperature);

        /// <summary>
        /// <para>
        /// In composites, gets the first layer.
        /// </para>
        /// <para>
        /// In other materials, gets the material itself.
        /// </para>
        /// </summary>
        /// <returns>The first layer of a composite, or the material itself.</returns>
        public IMaterial GetCore() => this;

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
        public IMaterial GetHomogenized() => this;

        /// <summary>Populates a <see cref="SerializationInfo"></see> with the data needed to
        /// serialize the target object.</summary>
        /// <param name="info">The <see cref="SerializationInfo"></see> to populate with
        /// data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"></see>) for this
        /// serialization.</param>
        /// <exception cref="System.Security.SecurityException">The caller does not have the
        /// required permission.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Constituents), Constituents);
            info.AddValue(nameof(Density), Density);
            info.AddValue(nameof(Mass), Mass);
            info.AddValue(nameof(Shape), Shape);
            info.AddValue(nameof(Temperature), Temperature);
        }

        /// <summary>
        /// <para>
        /// In composites, gets the last layer.
        /// </para>
        /// <para>
        /// In other materials, gets the material itself.
        /// </para>
        /// </summary>
        /// <returns>The last layer of a composite, or the material itself.</returns>
        public IMaterial GetSurface() => this;

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = -975444708;
            hashCode = (hashCode * -1521134295) + Density.GetHashCode();
            hashCode = (hashCode * -1521134295) + Mass.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<IShape>.Default.GetHashCode(Shape);
            hashCode = (hashCode * -1521134295) + EqualityComparer<IReadOnlyDictionary<ISubstanceReference, decimal>>.Default.GetHashCode(Constituents);
            return (hashCode * -1521134295) + (Temperature?.GetHashCode() ?? 0);
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
        public Material RemoveConstituent(ISubstanceReference substance)
        {
            if (!Constituents.TryGetValue(substance, out var proportion))
            {
                return this;
            }

            if (Constituents.Count == 1)
            {
                Density = 0;
                Mass = 0;
                Shape = SinglePoint.Origin;
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
        public Material RemoveConstituent(ISubstance substance)
        {
            if (!Constituents.TryGetValue(substance.GetReference(), out var proportion))
            {
                return this;
            }

            if (Constituents.Count == 1)
            {
                Density = 0;
                Mass = 0;
                Shape = SinglePoint.Origin;
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
        public Material RemoveConstituents(Predicate<ISubstance> match)
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
                Mass = 0;
                Shape = SinglePoint.Origin;
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
        /// A <see cref="Composite"/> whose components each have the same composition as this
        /// instance, with the specified proportions.
        /// </returns>
        /// <remarks>If the given <paramref name="proportions"/> do not sum to 1, they are
        /// normalized.</remarks>
        public IMaterial Split(params decimal[] proportions)
        {
            if (proportions.Length == 1 && (proportions[0] <= 0 || proportions[0] >= 1))
            {
                return this;
            }
            if (proportions.Length == 0)
            {
                proportions = new decimal[] { 0.5m, 0.5m };
            }
            else if (proportions.Length == 1)
            {
                proportions = new decimal[] { proportions[0], 1 - proportions[0] };
            }
            else
            {
                var sum = proportions.Sum();
                if (sum != 1)
                {
                    var ratio = 1 / sum;
                    for (var i = 0; i < proportions.Length; i++)
                    {
                        proportions[i] *= ratio;
                    }
                }
            }
            var components = new List<IMaterial>();
            for (var i = 0; i < proportions.Length; i++)
            {
                components.Add(GetClone(proportions[i]));
            }
            return new Composite(Shape, null, null, null, components);
        }
    }
}
