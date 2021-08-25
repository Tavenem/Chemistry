namespace Tavenem.Chemistry.Elements;

/// <summary>
/// Defines an atom's configuration of electrons.
/// </summary>
public readonly struct ElectronConfiguration : IEquatable<ElectronConfiguration>
{
    /// <summary>
    /// The orbitals involved in this configuration.
    /// </summary>
    public IReadOnlyCollection<Orbital> Orbitals { get; }

    /// <summary>
    /// Gets the total number of electrons in this configuration.
    /// </summary>
    public int TotalElectronCount => Orbitals.Sum(x => x.Number);

    /// <summary>
    /// Initializes a new instance of <see cref="ElectronConfiguration"/>.
    /// </summary>
    /// <param name="orbitals">The orbital(s) to include.</param>
    public ElectronConfiguration(params Orbital[] orbitals)
        => Orbitals = orbitals ?? Array.Empty<Orbital>();

    /// <summary>
    /// Initializes a new instance of <see cref="ElectronConfiguration"/>.
    /// </summary>
    /// <param name="source">A configuration to copy.</param>
    /// <param name="orbitals">Any orbital(s) to add to those found in <paramref
    /// name="source"/>.</param>
    public ElectronConfiguration(ElectronConfiguration source, params Orbital[] orbitals)
        => Orbitals = source.Orbitals.Concat(orbitals ?? Enumerable.Empty<Orbital>()).ToList();

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ElectronConfiguration other) => EqualityComparer<IReadOnlyCollection<Orbital>>.Default.Equals(Orbitals, other.Orbitals);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance are the same type
    /// and represent the same value; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is ElectronConfiguration other && Equals(other);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
        => unchecked(-1369406059 * -1521134295) + EqualityComparer<IReadOnlyCollection<Orbital>>.Default.GetHashCode(Orbitals);

    /// <summary>Returns a string equivalent of this instance.</summary>
    /// <returns>A string equivalent of this instance.</returns>
    public override string ToString() => string.Join(" ", Orbitals);

    /// <summary>
    /// Indicates whether two <see cref="ElectronConfiguration"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(ElectronConfiguration left, ElectronConfiguration right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two <see cref="ElectronConfiguration"/> instances are unequal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are unequal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(ElectronConfiguration left, ElectronConfiguration right) => !(left == right);
}
