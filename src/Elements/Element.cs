﻿namespace Tavenem.Chemistry.Elements;

/// <summary>
/// An element of the periodic table.
/// </summary>
public readonly struct Element : IEquatable<Element>
{
    /// <summary>
    /// The atomic number of the element.
    /// </summary>
    public byte AtomicNumber { get; }

    /// <summary>
    /// The average atomic mass of this element.
    /// </summary>
    public double AverageMass { get; }

    /// <summary>
    /// The average molar mass of this element, in kg/mol.
    /// </summary>
    public double AverageMolarMass => AverageMass * Mathematics.DoubleConstants.AvogadroConstant;

    /// <summary>
    /// The block this element occupies in the periodic table.
    /// </summary>
    public char Block { get; }

    /// <summary>
    /// The configuration of this element's electrons.
    /// </summary>
    public ElectronConfiguration ElectronConfiguration { get; }

    /// <summary>
    /// The element's IUAPC group number on the periodic table (<see langword="null"/> for the
    /// lanthanides and actinides).
    /// </summary>
    public byte? Group { get; }

    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The element's period on the periodic table.
    /// </summary>
    public byte Period { get; }

    /// <summary>
    /// The number of protons in this isotope.
    /// </summary>
    public byte Protons => AtomicNumber;

    /// <summary>
    /// The symbol for the element.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// The element's type(s) on the periodic table.
    /// </summary>
    public ElementType Type { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="Element"/>.
    /// </summary>
    /// <param name="atomicNumber">The atomic number of the element.</param>
    /// <param name="averageMass">The average atomic mass of this element, in kg.</param>
    /// <param name="block">The block this element ocupies in the periodic table.</param>
    /// <param name="electronConfiguration">The configuration of this element's
    /// electrons.</param>
    /// <param name="group">The element's IUAPC group number on the periodic table (<see
    /// langword="null"/> for the lanthanides and actinides).</param>
    /// <param name="name">The name of the element.</param>
    /// <param name="period">The element's period on the periodic table.</param>
    /// <param name="symbol">The symbol for the element.</param>
    /// <param name="type">The element's type(s) on the periodic table.</param>
    public Element(
        byte atomicNumber,
        double averageMass,
        char block,
        ElectronConfiguration electronConfiguration,
        byte? group,
        string name,
        byte period,
        string symbol,
        ElementType type)
    {
        AtomicNumber = atomicNumber;
        AverageMass = averageMass;
        Block = block;
        ElectronConfiguration = electronConfiguration;
        Group = group;
        Name = name;
        Period = period;
        Symbol = symbol;
        Type = type;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Element other) => AtomicNumber == other.AtomicNumber;

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance are the same type
    /// and represent the same value; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
        => obj is Element other && Equals(other);

    /// <summary>
    /// Gets the most abundant isotope of this element.
    /// </summary>
    /// <returns>The <see cref="Isotope"/> of this element which has the highest relative
    /// abundance (on Earth).</returns>
    public Isotope GetCommonIsotope() => PeriodicTable.GetCommonIsotope(this);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => unchecked(-2135874747 * -1521134295) + AtomicNumber.GetHashCode();

    /// <summary>Returns a string equivalent of this instance.</summary>
    /// <returns>A string equivalent of this instance.</returns>
    public override string ToString() => Name;

    /// <summary>
    /// Indicates whether two <see cref="Element"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(Element left, Element right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two <see cref="Element"/> instances are unequal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are unequal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(Element left, Element right) => !(left == right);

    /// <summary>
    /// Substitutes an <see cref="Element"/> instance with the most common isotope of the
    /// element.
    /// </summary>
    /// <param name="value">This instance.</param>
    public static implicit operator Isotope(Element value) => value.GetCommonIsotope();
}
