using System.ComponentModel;
using Tavenem.Mathematics;

namespace Tavenem.Chemistry.Elements;

/// <summary>
/// A particular isotope of an element.
/// </summary>
[TypeConverter(typeof(IsotopeConverter))]
public readonly struct Isotope : IEquatable<Isotope>
{
    /// <summary>
    /// An empty (default) isotope instance.
    /// </summary>
    public static readonly Isotope Empty;

    /// <summary>
    /// The atomic number of the element.
    /// </summary>
    public byte AtomicNumber => Element.AtomicNumber;

    /// <summary>
    /// <para>
    /// The atomic mass of this isotope, in atomic mass units.
    /// </para>
    /// <para>
    /// Contrast with <see cref="AverageMass"/>, which is the average atomic mass of the
    /// element, including all known isotopes according to their relative abundances.
    /// </para>
    /// </summary>
    public double AtomicMass
        => (Element.AtomicNumber * DoubleConstants.ProtonMass) + ((MassNumber - Element.Protons) * DoubleConstants.NeutronMass);

    /// <summary>
    /// <para>
    /// The average atomic mass of this element.
    /// </para>
    /// <para>
    /// Contrast with <see cref="AtomicMass"/>, which is the actual atomic mass of this
    /// particular isotope, rather than the average of all isotopes of the element.
    /// </para>
    /// </summary>
    public double AverageMass => Element.AverageMass;

    /// <summary>
    /// The average molar mass of this element, in kg/mol.
    /// </summary>
    public double AverageMolarMass => Element.AverageMolarMass;

    /// <summary>
    /// The block this element occupies in the periodic table.
    /// </summary>
    public char Block => Element.Block;

    /// <summary>
    /// The configuration of this element's electrons.
    /// </summary>
    public ElectronConfiguration ElectronConfiguration => Element.ElectronConfiguration;

    /// <summary>
    /// The element of which this is an isotope.
    /// </summary>
    public Element Element { get; }

    /// <summary>
    /// The element's IUAPC group number on the periodic table (<see langword="null"/> for the
    /// lanthanides and actinides).
    /// </summary>
    public byte? Group => Element.Group;

    /// <summary>
    /// Whether this isotope is radioactive.
    /// </summary>
    public bool IsRadioactive { get; }

    /// <summary>
    /// The mass number (number of nucleons) in this isotope.
    /// </summary>
    public ushort MassNumber { get; }

    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name => Element.Name;

    /// <summary>
    /// The number of neutrons in this isotope.
    /// </summary>
    public byte Neutrons => (byte)(MassNumber - Element.Protons);

    /// <summary>
    /// The element's period on the periodic table.
    /// </summary>
    public byte Period => Element.Period;

    /// <summary>
    /// The number of protons in this isotope.
    /// </summary>
    public byte Protons => Element.AtomicNumber;

    /// <summary>
    /// The relative abundance of this isotope (on Earth), as a normalized value between 0 and
    /// 1.
    /// </summary>
    public double RelativeAbundance { get; }

    /// <summary>
    /// The symbol for the element.
    /// </summary>
    public string Symbol => Element.Symbol;

    /// <summary>
    /// The element's type(s) on the periodic table.
    /// </summary>
    public ElementType Type => Element.Type;

    /// <summary>
    /// Initialize a new instance of <see cref="Isotope"/>.
    /// </summary>
    /// <param name="element">The element of which this is an isotope.</param>
    /// <param name="isRadioactive">Whether this isotope is radioactive.</param>
    /// <param name="massNumber">The mass number (number of nucleons) in this isotope.</param>
    /// <param name="relativeAbundance">The relative abundance of this isotope (on
    /// Earth).</param>
    public Isotope(
        Element element,
        bool isRadioactive,
        ushort massNumber,
        double relativeAbundance)
    {
        Element = element;
        IsRadioactive = isRadioactive;
        MassNumber = massNumber;
        RelativeAbundance = relativeAbundance;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Isotope other)
        => Element.Equals(other.Element)
        && MassNumber == other.MassNumber;

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance
    /// are the same type and represent the same value; otherwise, <see
    /// langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Isotope isotope && Equals(isotope);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
        var hashCode = -1835052901;
        hashCode = (hashCode * -1521134295) + Element.GetHashCode();
        return (hashCode * -1521134295) + MassNumber.GetHashCode();
    }

    /// <summary>Returns a string equivalent of this instance.</summary>
    /// <returns>A string equivalent of this instance.</returns>
    public override string ToString()
    {
        if (PeriodicTable.TryGetCommonIsotope(AtomicNumber, out var common)
            && MassNumber == common.MassNumber)
        {
            return Symbol;
        }
        else
        {
            return MassNumber.ToSuperscript() + Symbol;
        }
    }

    /// <summary>
    /// Indicates whether two <see cref="Isotope"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(Isotope left, Isotope right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two <see cref="Isotope"/> instances are unequal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are unequal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(Isotope left, Isotope right) => !(left == right);

    /// <summary>
    /// Substitutes an <see cref="Isotope"/> instance with its element.
    /// </summary>
    /// <param name="value">This instance.</param>
    public static implicit operator Element(Isotope value) => value.Element;
}
