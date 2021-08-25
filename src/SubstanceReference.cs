using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Tavenem.Chemistry;

/// <summary>
/// A reference to an <see cref="ISubstance"/> instance, which can be retrieved on demand from
/// the corresponding <see cref="Substances"/> registry.
/// </summary>
/// <remarks>
/// <para>
/// This object is expected to be used in place of an actual <see cref="ISubstance"/> value in
/// structures which are either persisted to memory or transmitted ovr the wire, when memory
/// footprint is a concern.
/// </para>
/// <para>
/// When serialized, this object retains only the <see cref="ISubstance.Name"/> of the <see
/// cref="ISubstance"/> it references. When any other property is accessed, the actual <see
/// cref="ISubstance"/> is retrieved from the corresponding <see cref="Substances"/> registry
/// and cached.
/// </para>
/// </remarks>
[TypeConverter(typeof(SubstanceReferenceConverter))]
[JsonConverter(typeof(ISubstanceReferenceConverter))]
public class SubstanceReference : ISubstanceReference
{
    /// <summary>
    /// An empty reference. Retrieves <see cref="Chemical.None"/>.
    /// </summary>
    public static readonly SubstanceReference Empty = new(string.Empty);

    /// <summary>
    /// The key used to retrieve the referenced <see cref="ISubstance"/> from the <see
    /// cref="Substances"/> registry.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// A string code used to prefix the <see cref="Id"/>.
    /// </summary>
    public string ReferenceCode => "SR";

    private ISubstance? _substance;
    /// <summary>
    /// The referenced <see cref="ISubstance"/>. May retrieve <see cref="Chemical.None"/> if the
    /// key is <see langword="null"/> or not found in the <see cref="Substances"/> registry.
    /// </summary>
    public ISubstance Substance => _substance ??= (Substances.TryGetSubstance(Id, out var substance) ? substance : Chemical.None);

    /// <summary>
    /// Initializes a new instance of <see cref="SubstanceReference"/>.
    /// </summary>
    /// <param name="id">The key used to retrieve the referenced <see cref="ISubstance"/> from
    /// the <see cref="Substances"/> registry.</param>
    public SubstanceReference(string? id) => Id = id ?? string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="SubstanceReference"/>.
    /// </summary>
    /// <param name="substance">The referenced <see cref="ISubstance"/>.</param>
    public SubstanceReference(ISubstance substance)
    {
        Id = substance.Id;
        _substance = substance;
    }

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ISubstanceReference? other)
        => other is not null && (Id?.Equals(other.Id, StringComparison.CurrentCulture) ?? false);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ISubstance? other)
        => other is not null && (Id?.Equals(other.Id, StringComparison.Ordinal) ?? false);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance
    /// are the same type and represent the same value; otherwise, <see
    /// langword="false"/>.</returns>
    public override bool Equals(object? obj)
        => (obj is ISubstance substance && Equals(substance))
        || (obj is ISubstanceReference reference && Equals(reference));

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => Id?.GetHashCode() ?? 0;

    /// <summary>Returns a string representation of this instance.</summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"{ReferenceCode}:{Id}";

    /// <summary>
    /// Indicates whether two substances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(SubstanceReference left, ISubstance right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two substances are unequal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are unequal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(SubstanceReference left, ISubstance right) => !(left == right);
}
