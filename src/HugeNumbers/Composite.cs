using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Tavenem.HugeNumbers;
using Tavenem.Mathematics.HugeNumbers;

namespace Tavenem.Chemistry.HugeNumbers
{
    /// <summary>
    /// A physical object composed of multiple individual materials in an overall shape.
    /// </summary>
    [Serializable]
    [DataContract]
    [System.Text.Json.Serialization.JsonConverter(typeof(CompositeConverter))]
    public class Composite : IMaterial, ISerializable, IEquatable<Composite>
    {
        /// <summary>
        /// The collection of this instance's material components.
        /// </summary>
        [DataMember(Order = 1)]
        public IReadOnlyList<IMaterial> Components { get; private set; }

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
                    var ratio = Mass.IsZero ? 0 : (decimal)(component.Mass / Mass);
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

        [DataMember(Name = nameof(Density), Order = 2)]
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
            get => _density ?? (double)(Components.Sum(x => x.Mass) / Components.Sum(x => x.Shape.Volume));
            set => _density = value;
        }

        /// <summary>
        /// Whether this material is an empty instance.
        /// </summary>
        public bool IsEmpty => Components.Count == 0;

        [DataMember(Name = nameof(Mass), Order = 3)]
        internal HugeNumber? _mass;
        /// <summary>
        /// <para>
        /// The mass of this material, in kg.
        /// </para>
        /// <para>
        /// If not set explicitly, will be the sum of the masses of its components.
        /// </para>
        /// </summary>
        public HugeNumber Mass
        {
            get => _mass ?? Components.Sum(x => x.Mass);
            set => _mass = value;
        }

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
        public IShape Shape { get; set; }

        [DataMember(Name = nameof(Temperature), Order = 5)]
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
                if (_temperature != null)
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
                    .Sum(x => x.Temperature * (double)(x.Mass / massTotal));
            }
            set => _temperature = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Composite"/>.
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
        /// <param name="mass">The mass of this material, in kg.</param>
        /// <param name="temperature">The average temperature of this material, in K. May be <see
        /// langword="null"/>, indicating that it is at the ambient temperature of its
        /// environment.</param>
        /// <param name="components">The components of this material. At least one is required.</param>
        public Composite(
            IShape shape,
            double? density = null,
            HugeNumber? mass = null,
            double? temperature = null,
            IEnumerable<IMaterial>? components = null) : this(components ?? Enumerable.Empty<IMaterial>(), shape, density, mass, temperature) { }

        /// <summary>
        /// Initializes a new instance of <see cref="Composite"/>.
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
        /// <param name="mass">The mass of this material, in kg.</param>
        /// <param name="temperature">The average temperature of this material, in K. May be <see
        /// langword="null"/>, indicating that it is at the ambient temperature of its
        /// environment.</param>
        /// <param name="components">The components of this material. At least one is required.</param>
        public Composite(
            IShape shape,
            double? density = null,
            HugeNumber? mass = null,
            double? temperature = null,
            params IMaterial[] components)
        {
            Shape = shape;
            _density = density;
            _mass = mass;
            _temperature = temperature;
            if (components is null || components.Length == 0)
            {
                throw new ArgumentException($"{nameof(components)} cannot be empty", nameof(components));
            }
            Components = components.ToList().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Composite"/>.
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
        /// <param name="mass">The mass of this material, in kg.</param>
        /// <param name="temperature">The average temperature of this material, in K. May be <see
        /// langword="null"/>, indicating that it is at the ambient temperature of its
        /// environment.</param>
        public Composite(
            IEnumerable<IMaterial> components,
            IShape shape,
            double? density = null,
            HugeNumber? mass = null,
            double? temperature = null)
        {
            Components = ImmutableList<IMaterial>.Empty.AddRange(components);
            if (Components.Count == 0)
            {
                throw new ArgumentException($"{nameof(components)} cannot be empty", nameof(components));
            }
            Shape = shape;
            _density = density;
            _mass = mass;
            _temperature = temperature;
        }

        private Composite(SerializationInfo info, StreamingContext context) : this(
            (IReadOnlyList<IMaterial>?)info.GetValue(nameof(Components), typeof(IReadOnlyList<IMaterial>)) ?? ImmutableList<IMaterial>.Empty,
            (IShape?)info.GetValue(nameof(Shape), typeof(IShape)) ?? SinglePoint.Origin,
            (double?)info.GetValue(nameof(Density), typeof(double?)),
            (HugeNumber?)info.GetValue(nameof(Mass), typeof(HugeNumber?)),
            (double?)info.GetValue(nameof(Temperature), typeof(double?)))
        { }

        /// <summary>
        /// Adds a component to this composite.
        /// </summary>
        /// <param name="material">The component to add.</param>
        /// <returns>This instance.</returns>
        public Composite AddComponent(IMaterial material)
        {
            var list = Components as ImmutableList<IMaterial>;
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
        /// substance will be adjusted to accomodate this value.
        /// </para>
        /// </param>
        /// <param name="index">The index at which to add the new component. A negative value
        /// indicates appending to the end of the list.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is greater than the
        /// length of the <see cref="Components"/> collection.</exception>
        public IMaterial AddComponent(IMaterial material, decimal proportion = 0.5m, int index = -1)
        {
            if (index > Components.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (proportion <= 0)
            {
                return this;
            }
            if (proportion >= 1)
            {
                return material;
            }

            var massTotal = Components.Sum(x => x.Mass);

            var ratio = 1 - proportion;
            var list = new List<IMaterial>();
            for (var i = 0; i < Components.Count; i++)
            {
                list.Add(Components[i].GetClone(ratio));
            }

            var targetMass = massTotal * proportion;
            var massFraction = (decimal)(targetMass / material.Mass);
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
        public Composite AddComponents(IEnumerable<IMaterial> materials)
        {
            var list = Components as ImmutableList<IMaterial>;
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
        public Composite AddComponents(params IMaterial[] materials)
        {
            var list = Components as ImmutableList<IMaterial>;
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
        public Composite AddConstituentToComponent(int index, IMaterial material)
        {
            if (index <= 0 || index >= Components.Count)
            {
                throw new IndexOutOfRangeException();
            }
            if (Components[index] is Composite composite)
            {
                composite.AddComponent(material);
            }
            else
            {
                var list = Components as ImmutableList<IMaterial>;
                if (list is not null)
                {
                    Components = list.SetItem(index, new Composite(Components[index].Shape, components: new IMaterial[] { Components[index], material }));
                }
            }
            return this;
        }

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        /// <remarks>See <see cref="GetClone(decimal)"/> for a strongly typed version of
        /// this method.</remarks>
        public object Clone() => GetClone(1);

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
        /// adjusted to accomodate this value.
        /// </para>
        /// </param>
        /// <returns>This instance.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than zero,
        /// or greater than or equal to the length of the <see cref="Components"/>
        /// collection.</exception>
        public IMaterial CopyComponent(int index, decimal proportion = 0.5m)
        {
            if (index < 0 || index >= Components.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (proportion <= 0)
            {
                return this;
            }
            if (proportion >= 1)
            {
                return Components[index];
            }

            var massTotal = Components.Sum(x => x.Mass);
            var targetMass = massTotal * proportion;
            var massFraction = (decimal)(targetMass / Components[index].Mass);
            var ratio = 1 - proportion;
            var list = new List<IMaterial>();
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
        public bool Equals(Composite? other)
            => other is not null
            && EqualityComparer<IShape>.Default.Equals(Shape, other.Shape)
            && EqualityComparer<HugeNumber?>.Default.Equals(_density, other._density)
            && EqualityComparer<HugeNumber?>.Default.Equals(_mass, other._mass)
            && EqualityComparer<HugeNumber?>.Default.Equals(_temperature, other._temperature)
            && Components.OrderBy(x => x.GetHashCode()).SequenceEqual(other.Components);

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="other">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="other">obj</paramref> and this instance
        /// are the same type and represent the same value; otherwise, <see
        /// langword="false"/>.</returns>
        public bool Equals(IMaterial? other) => other is Composite composite && Equals(composite);

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance
        /// are the same type and represent the same value; otherwise, <see
        /// langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is Composite composite && Equals(composite);

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
        public IMaterial GetClone(decimal massFraction = 1)
        {
            if (massFraction <= 0)
            {
                return Material.Empty;
            }
            else
            {
                HugeNumber? mass;
                if (_mass.HasValue)
                {
                    if (massFraction != 1)
                    {
                        mass = _mass * (HugeNumber)massFraction;
                    }
                    else
                    {
                        mass = _mass;
                    }
                }
                else
                {
                    mass = null;
                }

                return new Composite(
                  Shape,
                  _density,
                  mass,
                  _temperature,
                  Components.Select(x => x.GetClone(massFraction)));
            }
        }

        /// <summary>
        /// <para>
        /// In composites, gets the first layer.
        /// </para>
        /// <para>
        /// In other materials, gets the material itself.
        /// </para>
        /// </summary>
        /// <returns>The first layer of a composite, or the material itself.</returns>
        public IMaterial GetCore() => Components[0];

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = -1445643008;
            hashCode = (hashCode * -1521134295) + EqualityComparer<IShape>.Default.GetHashCode(Shape);
            hashCode = (hashCode * -1521134295) + (_density.HasValue ? EqualityComparer<HugeNumber>.Default.GetHashCode(_density.Value) : 0);
            hashCode = (hashCode * -1521134295) + (_mass.HasValue ? EqualityComparer<HugeNumber?>.Default.GetHashCode(_mass.Value) : 0);
            hashCode = (hashCode * -1521134295) + (_temperature.HasValue ? EqualityComparer<HugeNumber?>.Default.GetHashCode(_temperature.Value) : 0);
            return (hashCode * -1521134295) + EqualityComparer<IReadOnlyList<IMaterial>>.Default.GetHashCode(Components);
        }

        /// <summary>
        /// <para>
        /// In composites, gets a homogenized version of the mixture.
        /// </para>
        /// <para>
        /// In other materials, gets the material itself.
        /// </para>
        /// </summary>
        /// <returns>A homogenized version of a composites, or the material itself.</returns>
        public IMaterial GetHomogenized() => new Material(Constituents, Density, Mass, Shape, Temperature);

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
            info.AddValue(nameof(Shape), Shape);
            info.AddValue(nameof(Density), _density);
            info.AddValue(nameof(Mass), _mass);
            info.AddValue(nameof(Temperature), _temperature);
            info.AddValue(nameof(Components), Components);
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
        public IMaterial GetSurface() => Components[Components.Count - 1];

        /// <summary>
        /// Removes the given component from this composite.
        /// </summary>
        /// <param name="match">A condition to determine which components to remove.</param>
        /// <returns>This instance.</returns>
        public IMaterial RemoveAllComponents(Predicate<IMaterial> match)
        {
            var list = Components as ImmutableList<IMaterial>;
            if (list is not null)
            {
                Components = list.RemoveAll(match);
            }
            if (Components.Count == 0)
            {
                return Material.Empty;
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
        public IMaterial RemoveComponent(IMaterial material)
        {
            var list = Components as ImmutableList<IMaterial>;
            if (list is not null)
            {
                Components = list.Remove(material);
            }
            if (Components.Count == 0)
            {
                return Material.Empty;
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
        public IMaterial RemoveConstituentFromComponent(int index, IMaterial material)
        {
            if (index < 0 || index >= Components.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var removeLayer = false;
            var list = Components as ImmutableList<IMaterial>;
            if (list is not null)
            {
                if (list[index] is Composite composite)
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
        public IMaterial SetComponent(int index, IMaterial material, decimal proportion = 0.5m)
        {
            if (index < 0 || index >= Components.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (proportion <= 0)
            {
                return this;
            }
            if (proportion >= 1)
            {
                return material;
            }

            var massTotal = Components.Sum(x => x.Mass);

            var targetProportion = (decimal)(Components[index].Mass / massTotal);
            var ratio = 1 - (proportion - targetProportion);
            var targetMass = massTotal * proportion;
            var massFraction = (decimal)(targetMass / material.Mass);
            var list = new List<IMaterial>();
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
