using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Json.Serialization;
using Tavenem.Mathematics;

namespace Tavenem.Chemistry;

/// <summary>
/// A physical object composed of multiple individual materials in an overall shape.
/// </summary>
[JsonConverter(typeof(CompositeConverterFactory))]
public class Composite<TScalar> : IMaterial<Composite<TScalar>, TScalar>, IEquatable<Composite<TScalar>>
    where TScalar : IFloatingPointIeee754<TScalar>
{
    /// <summary>
    /// The collection of this instance's material components.
    /// </summary>
    public IReadOnlyList<IMaterial<TScalar>> Components { get; private set; }

    /// <summary>
    /// This material's constituent substances.
    /// </summary>
    public IReadOnlyDictionary<ISubstanceReference, decimal> Constituents
    {
        get
        {
            var constituents = new Dictionary<ISubstanceReference, decimal>();
            foreach (var component in Components)
            {
                var ratio = Mass.IsNearlyZero()
                    ? 0
                    : decimal.CreateChecked(component.Mass / Mass);
                foreach (var (substance, proportion) in component.Constituents)
                {
                    if (constituents.ContainsKey(substance))
                    {
                        constituents[substance] += proportion * ratio;
                    }
                    else
                    {
                        constituents[substance] = proportion * ratio;
                    }
                }
            }
            return new ReadOnlyDictionary<ISubstanceReference, decimal>(constituents);
        }
    }

    internal double? _density;
    /// <summary>
    /// <para>
    /// The average density of this material, in kg/m³.
    /// </para>
    /// <para>
    /// A material may have either uniform or uneven density (e.g. contained voids or an
    /// irregular shape contained within its overall dimensions). This value represents the
    /// average throughout the full volume of its <see cref="Shape"/>.
    /// </para>
    /// <para>
    /// If not set explicitly, will be the combined density of its components.
    /// </para>
    /// </summary>
    public double Density
    {
        get => _density ?? double.CreateChecked(Components.Sum(x => x.Mass) / Components.Sum(x => x.Shape.Volume));
        set => _density = value;
    }

    /// <summary>
    /// Whether this material is an empty instance.
    /// </summary>
    public bool IsEmpty => Components.Count == 0;

    internal TScalar? _mass;
    internal bool _hasMass;
    /// <summary>
    /// <para>
    /// The mass of this material, in kg.
    /// </para>
    /// <para>
    /// If not set explicitly, will be the sum of the masses of its components.
    /// </para>
    /// </summary>
    public TScalar Mass
    {
        get => _hasMass && _mass is not null
            ? _mass
            : Components.Sum(x => x.Mass);
        set
        {
            _mass = value;
            _hasMass = value is not null;
        }
    }

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
    public IShape<TScalar> Shape { get; set; }

    internal double? _temperature;
    /// <summary>
    /// <para>
    /// The average temperature of this material, in K. May be <see langword="null"/>,
    /// indicating that it is at the ambient temperature of its environment.
    /// </para>
    /// <para>
    /// If not set explicitly, will be the weighted average of its components, by mass.
    /// </para>
    /// </summary>
    public double? Temperature
    {
        get
        {
            if (_temperature.HasValue)
            {
                return _temperature;
            }
            if (!Components.Any(x => x.Temperature.HasValue))
            {
                return null;
            }
            var massTotal = Components.Sum(x => x.Mass);
            return Components
                .Where(x => x.Temperature.HasValue)
                .Sum(x => x.Temperature * double.CreateChecked(x.Mass / massTotal));
        }
        set => _temperature = value;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Composite{TScalar}"/>.
    /// </summary>
    /// <param name="components">The components of this material. At least one is required.</param>
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
    /// The average temperature of this material, in K. May be <see langword="null"/>, indicating
    /// that it is at the ambient temperature of its environment.
    /// </param>
    public Composite(
        IEnumerable<IMaterial<TScalar>> components,
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null)
    {
        Components = ImmutableList<IMaterial<TScalar>>.Empty.AddRange(components);
        if (Components.Count == 0)
        {
            throw new ArgumentException($"{nameof(components)} cannot be empty", nameof(components));
        }
        Shape = shape;
        Mass = mass;
        _density = density;
        _temperature = temperature;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Composite{TScalar}"/>.
    /// </summary>
    /// <param name="components">The components of this material. At least one is required.</param>
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
    /// <param name="temperature">The average temperature of this material, in K. May be <see
    /// langword="null"/>, indicating that it is at the ambient temperature of its
    /// environment.</param>
    public Composite(
        IEnumerable<IMaterial<TScalar>> components,
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null)
    {
        Components = ImmutableList<IMaterial<TScalar>>.Empty.AddRange(components);
        if (Components.Count == 0)
        {
            throw new ArgumentException($"{nameof(components)} cannot be empty", nameof(components));
        }
        Shape = shape;
        _mass = default;
        _density = density;
        _temperature = temperature;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Composite{TScalar}"/>.
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
    /// The average temperature of this material, in K. May be <see langword="null"/>, indicating
    /// that it is at the ambient temperature of its environment.
    /// </param>
    /// <param name="components">The components of this material. At least one is required.</param>
    public Composite(
        IShape<TScalar> shape,
        TScalar mass,
        double? density = null,
        double? temperature = null,
        params IMaterial<TScalar>[] components)
        : this(components.AsEnumerable(), shape, mass, density, temperature) { }

    /// <summary>
    /// Initializes a new instance of <see cref="Composite{TScalar}"/>.
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
    /// <param name="temperature">The average temperature of this material, in K. May be <see
    /// langword="null"/>, indicating that it is at the ambient temperature of its
    /// environment.</param>
    /// <param name="components">The components of this material. At least one is required.</param>
    public Composite(
        IShape<TScalar> shape,
        double? density = null,
        double? temperature = null,
        params IMaterial<TScalar>[] components)
        : this(components.AsEnumerable(), shape, density, temperature) { }

    /// <summary>
    /// Adds the given <paramref name="substance"/> as a new constituent of this material, in
    /// the given <paramref name="proportion"/>. Adds the <paramref name="substance"/> to each
    /// component evenly.
    /// </summary>
    /// <param name="substance">An <see cref="ISubstance"/> to add.</param>
    /// <param name="proportion">
    /// The proportion at which to add the <paramref name="substance"/>.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(ISubstanceReference substance, decimal proportion = 0.5m)
    {
        foreach (var component in Components)
        {
            component.Add(substance, proportion);
        }
        return this;
    }

    /// <summary>
    /// Adds the given <paramref name="substance"/> as a new constituent of this material, in the
    /// given <paramref name="proportion"/>. Adds the <paramref name="substance"/> to each
    /// component evenly.
    /// </summary>
    /// <param name="substance">An <see cref="ISubstance"/> to add.</param>
    /// <param name="proportion">
    /// The proportion at which to add the <paramref name="substance"/>.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(ISubstance substance, decimal proportion = 0.5m)
    => Add(substance.GetReference(), proportion);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's composition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(IEnumerable<(ISubstanceReference substance, decimal proportion)> constituents)
    {
        foreach (var component in Components)
        {
            component.Add(constituents);
        }
        return this;
    }

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's composition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(IEnumerable<(ISubstance substance, decimal proportion)> constituents)
        => Add(constituents);

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's composition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(params (ISubstanceReference substance, decimal proportion)[] constituents)
        => Add(constituents.AsEnumerable());

    /// <summary>
    /// Adds one or more new constituents to this material, at the given proportions.
    /// </summary>
    /// <param name="constituents">
    /// The new constituents to add, as a tuple of a substance and the proportion to assign to that
    /// substance. If a given substance already exists in this material's composition, its
    /// proportion is adjusted to the given value.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Add(params (ISubstance substance, decimal proportion)[] constituents)
        => Add(constituents.AsEnumerable());

    /// <summary>
    /// Adds a component to this composite.
    /// </summary>
    /// <param name="material">The component to add.</param>
    /// <returns>This instance.</returns>
    public Composite<TScalar> AddComponent(IMaterial<TScalar> material)
    {
        var list = Components as ImmutableList<IMaterial<TScalar>>;
        if (list is not null)
        {
            Components = list.Add(material);
        }
        return this;
    }

    /// <summary>
    /// Adds the given <paramref name="material"/> as a new component of this instance with the
    /// given <paramref name="proportion"/>.
    /// </summary>
    /// <param name="material">A substance to add as a new layer of this composite.</param>
    /// <param name="proportion">
    /// <para>
    /// The proportion at which to add the new component.
    /// </para>
    /// <para>
    /// The masses of the given <paramref name="material"/> and the existing components of this
    /// substance will be adjusted to accommodate this value.
    /// </para>
    /// </param>
    /// <param name="index">The index at which to add the new component. A negative value
    /// indicates appending to the end of the list.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is greater than the
    /// length of the <see cref="Components"/> collection.</exception>
    public IMaterial<TScalar> AddComponent(IMaterial<TScalar> material, TScalar proportion, int index = -1)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Components.Count);

        if (proportion <= TScalar.Zero)
        {
            return this;
        }
        if (proportion >= TScalar.One)
        {
            return material;
        }

        var massTotal = Components.Sum(x => x.Mass);

        var ratio = TScalar.One - proportion;
        var list = new List<IMaterial<TScalar>>();
        for (var i = 0; i < Components.Count; i++)
        {
            list.Add(Components[i].GetClone(ratio));
        }

        var targetMass = massTotal * proportion;
        var massFraction = targetMass / material.Mass;
        if (index < 0)
        {
            list.Add(material.GetClone(massFraction));
        }
        else
        {
            list.Insert(index, material.GetClone(massFraction));
        }
        Components = list;

        return this;
    }

    /// <summary>
    /// Adds a component to this composite.
    /// </summary>
    /// <param name="materials">The components to add.</param>
    /// <returns>This instance.</returns>
    public Composite<TScalar> AddComponents(IEnumerable<IMaterial<TScalar>> materials)
    {
        var list = Components as ImmutableList<IMaterial<TScalar>>;
        if (list is not null)
        {
            Components = list.AddRange(materials);
        }
        return this;
    }

    /// <summary>
    /// Adds a component to this composite.
    /// </summary>
    /// <param name="materials">The components to add.</param>
    /// <returns>This instance.</returns>
    public Composite<TScalar> AddComponents(params IMaterial<TScalar>[] materials)
    {
        var list = Components as ImmutableList<IMaterial<TScalar>>;
        if (list is not null)
        {
            Components = list.AddRange(materials);
        }
        return this;
    }

    /// <summary>
    /// Adds the given <paramref name="material"/> to the component of this instance at the
    /// given <paramref name="index"/>. If the indicated component is not already a composite,
    /// it becomes one.
    /// </summary>
    /// <param name="index">The zero-based index of the component to which the given <paramref
    /// name="material"/> will be added.</param>
    /// <param name="material">A substance to add as a constituent of this instance's indicated
    /// component.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than zero,
    /// or greater than or equal to the length of the <see cref="Components"/>
    /// collection.</exception>
    public Composite<TScalar> AddConstituentToComponent(int index, IMaterial<TScalar> material)
    {
        if (index <= 0 || index >= Components.Count)
        {
            throw new IndexOutOfRangeException();
        }
        if (Components[index] is Composite<TScalar> composite)
        {
            composite.AddComponent(material);
        }
        else
        {
            var list = Components as ImmutableList<IMaterial<TScalar>>;
            if (list is not null)
            {
                Components = list.SetItem(index, new Composite<TScalar>(Components[index].Shape, components: new IMaterial<TScalar>[] { Components[index], material }));
            }
        }
        return this;
    }

    /// <summary>Creates a new object that is a copy of the current instance.</summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    /// <remarks>See <see cref="GetClone(TScalar)"/> for a strongly typed version of
    /// this method.</remarks>
    public object Clone() => GetTypedClone();

    /// <summary>
    /// Copies the component at the given index and append it to the end of the collection, with
    /// the given <paramref name="proportion"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the component which will be
    /// duplicated.</param>
    /// <param name="proportion">
    /// <para>
    /// The proportion at which to add the copied component.
    /// </para>
    /// <para>
    /// The masses of the new component and the existing components of this substance will be
    /// adjusted to accommodate this value.
    /// </para>
    /// </param>
    /// <returns>This instance.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than zero,
    /// or greater than or equal to the length of the <see cref="Components"/>
    /// collection.</exception>
    public IMaterial<TScalar> CopyComponent(int index, TScalar proportion)
    {
        if (index < 0 || index >= Components.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (proportion <= TScalar.Zero)
        {
            return this;
        }
        if (proportion >= TScalar.One)
        {
            return Components[index];
        }

        var massTotal = Components.Sum(x => x.Mass);
        var targetMass = massTotal * proportion;
        var massFraction = targetMass / Components[index].Mass;
        var ratio = TScalar.One - proportion;
        var list = new List<IMaterial<TScalar>>();
        for (var i = 0; i < Components.Count; i++)
        {
            list.Add(Components[i].GetClone(ratio));
        }

        if (index < 0)
        {
            list.Add(Components[index].GetClone(massFraction));
        }
        else
        {
            list.Insert(index, Components[index].GetClone(massFraction));
        }
        Components = list;

        return this;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Composite<TScalar>? other)
        => other is not null
        && EqualityComparer<IShape<TScalar>>.Default.Equals(Shape, other.Shape)
        && _density == other._density
        && (_mass is null) == (other._mass is null)
        && (_mass is null
        || (other._mass is not null
        && _mass == other._mass))
        && _temperature == other._temperature
        && Components.OrderBy(x => x.GetHashCode()).SequenceEqual(other.Components);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="other">obj</paramref> and this instance
    /// are the same type and represent the same value; otherwise, <see
    /// langword="false"/>.</returns>
    public bool Equals(IMaterial<TScalar>? other) => other is Composite<TScalar> composite && Equals(composite);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance
    /// are the same type and represent the same value; otherwise, <see
    /// langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Composite<TScalar> composite && Equals(composite);

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
    public IMaterial<TScalar> GetClone(TScalar massFraction)
    {
        if (massFraction <= TScalar.Zero)
        {
            return Material<TScalar>.Empty;
        }
        else
        {
            if (_hasMass && _mass is not null)
            {
                var mass = massFraction != TScalar.One
                    ? _mass * massFraction
                    : _mass;
                return new Composite<TScalar>(
                    Components.Select(x => x.GetClone(massFraction)),
                    Shape,
                    mass,
                    _density,
                    _temperature);
            }

            return new Composite<TScalar>(
                Components.Select(x => x.GetClone(massFraction)),
                Shape,
                _density,
                _temperature);
        }
    }

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
    public IMaterial<TScalar> GetCore() => Components[0];

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(Components, Shape, _mass, _hasMass, _density, _temperature);

    /// <summary>
    /// <para>
    /// In composites, gets a homogenized version of the mixture.
    /// </para>
    /// <para>
    /// In other materials, gets the material itself.
    /// </para>
    /// </summary>
    /// <returns>A homogenized version of a composite, or the material itself.</returns>
    public IMaterial<TScalar> GetHomogenized()
        => new Material<TScalar>(Constituents, Density, Mass, Shape, Temperature);

    /// <summary>
    /// <para>
    /// In composites, gets the last layer.
    /// </para>
    /// <para>
    /// In other materials, gets the material itself.
    /// </para>
    /// </summary>
    /// <returns>The last layer of a composite, or the material itself.</returns>
    public IMaterial<TScalar> GetSurface() => Components[Components.Count - 1];

    /// <summary>
    /// Gets a deep clone of this <see cref="IMaterial{TSelf, TScalar}"/> instance.
    /// </summary>
    /// <returns>A deep clone of this instance.</returns>
    public Composite<TScalar> GetTypedClone()
    {
        if (_hasMass && _mass is not null)
        {
            return new Composite<TScalar>(
                Components.Select(x => x.GetClone()),
                Shape,
                _mass,
                _density,
                _temperature);
        }

        return new Composite<TScalar>(
            Components.Select(x => x.GetClone()),
            Shape,
            _density,
            _temperature);
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
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> that defines the conditions of the substances to remove.
    /// </param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> Remove(Predicate<ISubstance> match)
    {
        foreach (var component in Components)
        {
            component.Remove(match);
        }
        return this;
    }

    /// <summary>
    /// Removes the given component from this composite.
    /// </summary>
    /// <param name="match">A condition to determine which components to remove.</param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> RemoveAllComponents(Predicate<IMaterial<TScalar>> match)
    {
        var list = Components as ImmutableList<IMaterial<TScalar>>;
        if (list is not null)
        {
            Components = list.RemoveAll(match);
        }
        if (Components.Count == 0)
        {
            return Material<TScalar>.Empty;
        }
        else if (Components.Count == 1)
        {
            return Components[0];
        }
        else
        {
            return this;
        }
    }

    /// <summary>
    /// Removes the given component from this composite.
    /// </summary>
    /// <param name="material">A component to remove from this instance.</param>
    /// <returns>This instance.</returns>
    public IMaterial<TScalar> RemoveComponent(IMaterial<TScalar> material)
    {
        var list = Components as ImmutableList<IMaterial<TScalar>>;
        if (list is not null)
        {
            Components = list.Remove(material);
        }
        if (Components.Count == 0)
        {
            return Material<TScalar>.Empty;
        }
        else if (Components.Count == 1)
        {
            return Components[0];
        }
        else
        {
            return this;
        }
    }

    /// <summary>
    /// Removes the given <paramref name="material"/> from the component with the given
    /// <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the component from which the given <paramref
    /// name="material"/> will be removed.</param>
    /// <param name="material">A substance to remove.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than zero,
    /// or greater than or equal to the length of the <see cref="Components"/>
    /// collection.</exception>
    public IMaterial<TScalar> RemoveConstituentFromComponent(int index, IMaterial<TScalar> material)
    {
        if (index < 0 || index >= Components.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        var removeLayer = false;
        var list = Components as ImmutableList<IMaterial<TScalar>>;
        if (list is not null)
        {
            if (list[index] is Composite<TScalar> composite)
            {
                list = list.SetItem(index, composite.RemoveComponent(material));
                if (list[index].IsEmpty)
                {
                    removeLayer = true;
                }
            }
            else if (list[index].Equals(material))
            {
                removeLayer = true;
            }
            if (removeLayer)
            {
                Components = list.RemoveAt(index);
            }
        }
        return this;
    }

    /// <summary>
    /// Replaces the component at the indicated <paramref name="index"/> with the given <paramref
    /// name="material"/>, at the new <paramref name="proportion"/>.
    /// </summary>
    /// <param name="index">The index of the component to replace.</param>
    /// <param name="material">The substance with which to replace the indicated component.</param>
    /// <param name="proportion">
    /// <para>
    /// The new proportion of the replaced component.
    /// </para>
    /// </param>
    /// <returns>This instance.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than zero,
    /// or greater than or equal to the length of the <see cref="Components"/>
    /// collection.</exception>
    public IMaterial<TScalar> SetComponent(int index, IMaterial<TScalar> material, TScalar proportion)
    {
        if (index < 0 || index >= Components.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (proportion <= TScalar.Zero)
        {
            return this;
        }
        if (proportion >= TScalar.One)
        {
            return material;
        }

        var massTotal = Components.Sum(x => x.Mass);

        var targetProportion = Components[index].Mass / massTotal;
        var ratio = TScalar.One - (proportion - targetProportion);
        var targetMass = massTotal * proportion;
        var massFraction = targetMass / material.Mass;
        var list = new List<IMaterial<TScalar>>();
        for (var i = 0; i < Components.Count; i++)
        {
            if (i == index)
            {
                list.Add(material.GetClone(massFraction));
            }
            else
            {
                list.Add(Components[i].GetClone(ratio));
            }
        }

        if (index < 0)
        {
            list.Add(material.GetClone(massFraction));
        }
        else
        {
            list.Insert(index, material.GetClone(massFraction));
        }
        Components = list;

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
        if (proportions.Length == 1 && (proportions[0] <= TScalar.Zero || proportions[0] >= TScalar.One))
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
