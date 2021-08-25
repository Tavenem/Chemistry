namespace Tavenem.Chemistry.Elements;

/// <summary>
/// <para>
/// The element category within the periodic table to which an element belongs.
/// </para>
/// <para>
/// A <see cref="FlagsAttribute"/> enum.
/// </para>
/// </summary>
[Flags]
public enum ElementType
{
    /// <summary>
    /// Any type. Used for matching, rather than to specify the type of a particular substance.
    /// </summary>
    Any = ~0,

    /// <summary>
    /// No type. Used for matching, rather than to specify the type of a particular substance.
    /// </summary>
    None = 0,

    /// <summary>
    /// An alkali metal.
    /// </summary>
    Alkali = 1 << 0,

    /// <summary>
    /// An alkaline earth metal.
    /// </summary>
    AlkalineEarth = 1 << 1,

    /// <summary>
    /// A transition metal.
    /// </summary>
    Transition = 1 << 2,

    /// <summary>
    /// Scandium and Yttrium.
    /// </summary>
    Group3 = 1 << 3,

    /// <summary>
    /// A post-transition metal.
    /// </summary>
    PostTransition = 1 << 4,

    /// <summary>
    /// A lanthanide metal.
    /// </summary>
    Lanthanide = 1 << 5,

    /// <summary>
    /// An actinide metal.
    /// </summary>
    Actinide = 1 << 6,

    /// <summary>
    /// A rare-earth metal.
    /// </summary>
    RareEarth = Group3 | Lanthanide | Actinide,

    /// <summary>
    /// A metal.
    /// </summary>
    Metal = Alkali | AlkalineEarth | Transition | PostTransition | Lanthanide | Actinide,

    /// <summary>
    /// A reactive nonmetal.
    /// </summary>
    ReactiveNonmetal = 1 << 7,

    /// <summary>
    /// A noble gas.
    /// </summary>
    NobleGas = 1 << 8,

    /// <summary>
    /// A nonmetal.
    /// </summary>
    Nonmetal = ReactiveNonmetal | NobleGas,

    /// <summary>
    /// A metalloid.
    /// </summary>
    Metalloid = 1 << 9,

    /// <summary>
    /// A member of the Nitrogen family (group 15).
    /// </summary>
    Pnictogen = 1 << 10,

    /// <summary>
    /// A member of the Oxygen family (group 16).
    /// </summary>
    Chalcogen = 1 << 11,

    /// <summary>
    /// A member of group 17.
    /// </summary>
    Halogen = 1 << 12,
}
