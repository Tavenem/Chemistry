namespace Tavenem.Chemistry.Elements;

/// <summary>
/// Defines an orbital of electrons.
/// </summary>
public readonly struct Orbital : IEquatable<Orbital>
{
    /// <summary>
    /// The number of electrons.
    /// </summary>
    public byte Number { get; }

    /// <summary>
    /// The energy level, principal quantum number.
    /// </summary>
    public byte Shell { get; }

    /// <summary>
    /// The shape, subshell.
    /// </summary>
    public char Type { get; }

    /// <summary>
    /// Initialize a new instance of <see cref="Orbital"/>.
    /// </summary>
    /// <param name="shell">The energy level, principal quantum number.</param>
    /// <param name="type">The shape, subshell.</param>
    /// <param name="number">The number of electrons.</param>
    public Orbital(byte shell, char type, byte number)
    {
        Number = number;
        Shell = shell;
        Type = type;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Orbital other) => Number == other.Number && Shell == other.Shell && Type == other.Type;

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance are the same type
    /// and represent the same value; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Orbital orbital && Equals(orbital);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
        var hashCode = 1302144758;
        hashCode = (hashCode * -1521134295) + Number.GetHashCode();
        hashCode = (hashCode * -1521134295) + Shell.GetHashCode();
        return (hashCode * -1521134295) + Type.GetHashCode();
    }

    /// <summary>Returns a string equivalent of this instance.</summary>
    /// <returns>A string equivalent of this instance.</returns>
    public override string ToString() => Shell.ToString() + Type.ToString() + Number.ToSuperscript();

    /// <summary>
    /// Indicates whether two <see cref="Orbital"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(Orbital left, Orbital right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two <see cref="Orbital"/> instances are unequal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are unequal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(Orbital left, Orbital right) => !(left == right);
}
