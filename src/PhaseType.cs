namespace Tavenem.Chemistry;

/// <summary>
/// <para>
/// The phase of matter.
/// </para>
/// <para>
/// A <see cref="FlagsAttribute"/> enum.
/// </para>
/// </summary>
[Flags]
public enum PhaseType
{
    /// <summary>
    /// Any phase. Used for matching, rather than to specify the phase of a particular
    /// substance.
    /// </summary>
    Any = ~0,

    /// <summary>
    /// No phase. Used for matching, rather than to specify the phase of a particular
    /// substance.
    /// </summary>
    None = 0,

    /// <summary>
    /// The solid phase.
    /// </summary>
    Solid = 1 << 0,

    /// <summary>
    /// The liquid phase.
    /// </summary>
    Liquid = 1 << 1,

    /// <summary>
    /// The gas phase.
    /// </summary>
    Gas = 1 << 2,

    /// <summary>
    /// The plasma phase.
    /// </summary>
    Plasma = 1 << 3,

    /// <summary>
    /// A non-crystalline, amorphous solid.
    /// </summary>
    Glass = 1 << 4,

    /// <summary>
    /// Flowing yet ordered matter.
    /// </summary>
    LiquidCrystal = 1 << 5,

    /// <summary>
    /// Matter in a single quantum state with a uniform waveform.
    /// </summary>
    BoseEinsteinCondensate = 1 << 6,

    /// <summary>
    /// Dense atomic material with free electrons.
    /// </summary>
    ElectronDegenerateMatter = 1 << 7,

    /// <summary>
    /// Superdense neutrons.
    /// </summary>
    NeutronDegenerateMatter = 1 << 8,
}
