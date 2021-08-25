using System.Text.Json.Serialization;

namespace Tavenem.Chemistry;

/// <summary>
/// A homogeneous substance, whether a single chemical compound, or a homogeneous solution.
/// </summary>
[JsonConverter(typeof(ISubstanceConverter))]
public interface IHomogeneous : ISubstance, IEquatable<HomogeneousReference>
{
    /// <summary>
    /// The "A" Antoine coefficient which can be used to determine the vapor pressure of this substance.
    /// </summary>
    double? AntoineCoefficientA { get; }

    /// <summary>
    /// The "B" Antoine coefficient which can be used to determine the vapor pressure of this substance.
    /// </summary>
    double? AntoineCoefficientB { get; }

    /// <summary>
    /// The "C" Antoine coefficient which can be used to determine the vapor pressure of this substance.
    /// </summary>
    double? AntoineCoefficientC { get; }

    /// <summary>
    /// The upper limit of the Antoine coefficients' accuracy for this substance. It is presumed
    /// reasonable to assume that the substance always vaporizes above this temperature.
    /// </summary>
    double? AntoineMaximumTemperature { get; }

    /// <summary>
    /// The lower limit of the Antoine coefficients' accuracy for this substance. It is presumed
    /// reasonable to assume that the substance always condenses below this temperature.
    /// </summary>
    double? AntoineMinimumTemperature { get; }

    /// <summary>
    /// If set, indicates an explicitly defined phase for this substance, which overrides the
    /// usual phase calculations based on temperature and pressure.
    /// </summary>
    /// <remarks>
    /// This is expected to be utilized mainly for substances in exotic phases of matter, such
    /// as plasma, glass, etc. These phases are not indicated using the standard <see
    /// cref="GetPhase(double, double)"/> method.
    /// </remarks>
    PhaseType? FixedPhase { get; }

    /// <summary>
    /// The melting point of this substance at 100 kPa, in K.
    /// </summary>
    double? MeltingPoint { get; }

    /// <summary>
    /// Gets an <see cref="HomogeneousReference"/> for this <see cref="IHomogeneous"/>.
    /// </summary>
    /// <returns>An <see cref="HomogeneousReference"/> for this <see
    /// cref="IHomogeneous"/>.</returns>
    HomogeneousReference GetHomogeneousReference();

    /// <summary>
    /// Calculates the phase of this substance under the given conditions of temperature and
    /// pressure.
    /// </summary>
    /// <param name="temperature">The temperature, in K.</param>
    /// <param name="pressure">The pressure, in kPa.</param>
    /// <returns>The phase of this substance under the given conditions.</returns>
    /// <remarks>
    /// Only the solid, liquid, and gas phases are considered. Exotic phases of matter must be
    /// indicated explicitly via <see cref="FixedPhase"/>; they will never be calculated by this
    /// method.
    /// </remarks>
    PhaseType GetPhase(double temperature, double pressure);

    /// <summary>
    /// Calculates the vapor pressure of this substance, in kPa.
    /// </summary>
    /// <param name="temperature">The temperature, in K.</param>
    /// <returns>The vapor pressure of this substance, in kPa, or <see langword="null"/> if the
    /// Antoine coefficients have not been set for this substance.</returns>
    /// <remarks>
    /// <para>
    /// Uses Antoine's equation. If Antoine coefficients have not been explicitly set for this
    /// chemical, the return value will be null.
    /// </para>
    /// <para>
    /// If the indicated <paramref name="temperature"/> is beyond the indicated range of the
    /// Antoine coefficients (via <see cref="AntoineMinimumTemperature"/> and/or <see
    /// cref="AntoineMaximumTemperature"/>), the result may be <see
    /// cref="double.PositiveInfinity"/> (if the temperature is above the maximum), or <see
    /// cref="double.NegativeInfinity"/> (if the temperature is below the minimum).
    /// </para>
    /// </remarks>
    double? GetVaporPressure(double temperature);
}
