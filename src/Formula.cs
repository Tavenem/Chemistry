using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Tavenem.Chemistry.Elements;
using Tavenem.Mathematics;

namespace Tavenem.Chemistry;

/// <summary>
/// <para>
/// The chemical formula of a substance.
/// </para>
/// <para>
/// Internally records only the number of each nuclide (i.e. the data recorded by Hill
/// notation). Does not preserve any information about structure implied by a parsed formula
/// string.
/// </para>
/// <para>
/// Each element is recorded internally as a specific nuclide, even when no specific mass number
/// is mentioned in a parsed formula string (the most common isotope is assumed in that case).
/// </para>
/// </summary>
[JsonConverter(typeof(FormulaConverter))]
public readonly struct Formula : IEquatable<Formula>
{
    /// <summary>
    /// A <see cref="Formula"/> with no contents, and zero <see cref="Charge"/>.
    /// </summary>
    public static readonly Formula Empty;

    private readonly Dictionary<string, ushort> _isotopes;

    /// <summary>
    /// <para>
    /// This formula's average mass.
    /// </para>
    /// <para>
    /// Ignores specific isotopes in the formula, using the average mass of each element.
    /// </para>
    /// </summary>
    public double AverageMass => _isotopes?.Sum(x => (PeriodicTable.TryGetIsotope(x.Key, out var isotope) ? isotope.AverageMass : 0) * x.Value) ?? 0;

    /// <summary>
    /// This formula's ionic charge, as an integral multiple of the elementary charge.
    /// </summary>
    public short Charge { get; }

    /// <summary>
    /// Enumerates the unique elements included in this formula.
    /// </summary>
    public IEnumerable<Element> Elements => PeriodicTable.GetIsotopes(_isotopes?.Select(x => x.Key)).Select(x => x.Element).Distinct();

    /// <summary>
    /// Whether this formula contains no nuclides.
    /// </summary>
    public bool IsEmpty => (_isotopes?.Count ?? 0) == 0;

    /// <summary>
    /// <para>
    /// Enumerates the unique nuclides included in this formula.
    /// </para>
    /// <para>
    /// N.B. this is not the nuclides which are not the most common isotope of a particular
    /// element.
    /// </para>
    /// </summary>
    public IEnumerable<Isotope> Isotopes => PeriodicTable.GetIsotopes(_isotopes?.Select(x => x.Key));

    /// <summary>
    /// This formula's monoisotopic mass, in atomic mass units.
    /// </summary>
    public double MonoisotopicMass => _isotopes?.Sum(x => (PeriodicTable.TryGetIsotope(x.Key, out var isotope) ? isotope.AtomicMass : 0) * x.Value) ?? 0;

    /// <summary>
    /// Enumerates the unique nuclides included in this formula, along with their amounts.
    /// </summary>
    public IEnumerable<(Isotope nuclide, ushort amount)> Nuclides
        => _isotopes?.Select(x => (PeriodicTable.TryGetIsotope(x.Key, out var isotope) ? isotope : Isotope.Empty, x.Value)) ?? Enumerable.Empty<(Isotope nuclide, ushort amount)>();

    /// <summary>
    /// The number of atoms in this formula.
    /// </summary>
    public int NumberOfAtoms => _isotopes?.Sum(x => x.Value) ?? 0;

    /// <summary>
    /// Initializes a new instance of <see cref="Formula"/>.
    /// </summary>
    /// <param name="charge">This formula's ionic charge, as an integral multiple of the
    /// elementary charge.</param>
    /// <param name="isotopes">The isotopes present in the formula.</param>
    public Formula(short charge, Dictionary<string, ushort> isotopes)
    {
        _isotopes = isotopes;
        Charge = charge;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Formula"/>.
    /// </summary>
    /// <param name="charge">This formula's ionic charge, as an integral multiple of the
    /// elementary charge.</param>
    /// <param name="isotopes">The isotopes present in the formula.</param>
    public Formula(short charge, Dictionary<Isotope, ushort> isotopes)
    {
        _isotopes = isotopes.ToDictionary(x => IsotopeConverter.ConvertToString(x.Key), x => x.Value);
        Charge = charge;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Formula"/>.
    /// </summary>
    /// <param name="isotopes">The isotopes present in the formula.</param>
    public Formula(Dictionary<string, ushort> isotopes) : this(0, isotopes) { }

    /// <summary>
    /// Initializes a new instance of <see cref="Formula"/>.
    /// </summary>
    /// <param name="isotopes">The isotopes present in the formula.</param>
    public Formula(Dictionary<Isotope, ushort> isotopes) : this(0, isotopes) { }

    /// <summary>
    /// Initializes a new instance of <see cref="Formula"/>.
    /// </summary>
    /// <param name="charge">This formula's ionic charge, as an integral multiple of the
    /// elementary charge.</param>
    /// <param name="isotopes">The isotopes present in the formula, along with their number.</param>
    public Formula(short charge, params (Isotope isotope, ushort number)[] isotopes)
        : this(charge, isotopes.Where(x => x.number > 0).ToDictionary(x => IsotopeConverter.ConvertToString(x.isotope), x => x.number)) { }

    /// <summary>
    /// Initializes a new instance of <see cref="Formula"/>.
    /// </summary>
    /// <param name="isotopes">The isotopes present in the formula. Isotopes may be
    /// repeated.</param>
    public Formula(params (Isotope isotope, ushort number)[] isotopes) : this(0, isotopes) { }

    /// <summary>
    /// Initializes a new instance of <see cref="Formula"/>.
    /// </summary>
    /// <param name="charge">This formula's ionic charge, as an integral multiple of the
    /// elementary charge.</param>
    /// <param name="isotopes">
    /// <para>
    /// The isotopes present in the formula.
    /// </para>
    /// <para>
    /// Only one of each will be present. To include more than one on any isotope, use one of
    /// the alternate constructors.
    /// </para>
    /// </param>
    public Formula(short charge, params Isotope[] isotopes)
        : this(charge, isotopes.ToDictionary(x => IsotopeConverter.ConvertToString(x), _ => (ushort)1)) { }

    /// <summary>
    /// Initializes a new instance of <see cref="Formula"/>.
    /// </summary>
    /// <param name="isotopes">
    /// <para>
    /// The isotopes present in the formula.
    /// </para>
    /// <para>
    /// Only one of each will be present. To include more than one on any isotope, use one of
    /// the alternate constructors.
    /// </para>
    /// </param>
    public Formula(params Isotope[] isotopes) : this(0, isotopes) { }

    /// <summary>
    /// <para>
    /// Adds two formulas.
    /// </para>
    /// <para>
    /// Performs simple summation of the numbers of nuclides. No chemical analysis or checking
    /// is performed to validate that the combination is possible. Does not represent a chemical
    /// equation.
    /// </para>
    /// </summary>
    /// <param name="first">The first formula to combine.</param>
    /// <param name="second">The second formula to combine.</param>
    /// <returns>A combined formula.</returns>
    /// <exception cref="OverflowException">The combined charges of the formulas exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula Add(Formula first, Formula second)
    {
        var combinedIsotopes = new Dictionary<string, ushort>(first._isotopes);
        foreach (var isotope in second._isotopes)
        {
            if (combinedIsotopes.ContainsKey(isotope.Key))
            {
                combinedIsotopes[isotope.Key] += isotope.Value;
            }
            else
            {
                combinedIsotopes[isotope.Key] = isotope.Value;
            }
        }
        return new Formula((short)(first.Charge + second.Charge), combinedIsotopes);
    }

    /// <summary>
    /// Get the given formula, with all its nuclide counts divided by the given <paramref
    /// name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to divide.</param>
    /// <param name="divisor">The factor by which to divide the formula. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula Divide(Formula value, decimal divisor)
    {
        if (divisor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(divisor));
        }
        var combined = new Dictionary<string, ushort>(value._isotopes);
        foreach (var isotope in combined.ToList())
        {
            var amount = (int)Math.Round(isotope.Value / divisor);
            if (amount == 0)
            {
                combined.Remove(isotope.Key);
            }
            else
            {
                combined[isotope.Key] = (ushort)amount;
            }
        }
        return new Formula((short)Math.Round(value.Charge / divisor), combined);
    }

    /// <summary>
    /// Get the given formula, with all its nuclide counts divided by the given <paramref
    /// name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to divide.</param>
    /// <param name="divisor">The factor by which to divide the formula. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula Divide(Formula value, double divisor)
    {
        if (divisor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(divisor));
        }
        var combined = new Dictionary<string, ushort>(value._isotopes);
        foreach (var isotope in combined.ToList())
        {
            var amount = (int)Math.Round(isotope.Value / divisor);
            if (amount == 0)
            {
                combined.Remove(isotope.Key);
            }
            else
            {
                combined[isotope.Key] = (ushort)amount;
            }
        }
        return new Formula((short)Math.Round(value.Charge / divisor), combined);
    }

    /// <summary>
    /// Get the given formula, with all its nuclide counts divided by the given <paramref
    /// name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to divide.</param>
    /// <param name="divisor">The factor by which to divide the formula. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula Divide(Formula value, float divisor)
    {
        if (divisor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(divisor));
        }
        var combined = new Dictionary<string, ushort>(value._isotopes);
        foreach (var isotope in combined.ToList())
        {
            var amount = (int)Math.Round(isotope.Value / divisor);
            if (amount == 0)
            {
                combined.Remove(isotope.Key);
            }
            else
            {
                combined[isotope.Key] = (ushort)amount;
            }
        }
        return new Formula((short)Math.Round(value.Charge / divisor), combined);
    }

    /// <summary>
    /// Get the given formula, with all its nuclide counts divided by the given <paramref
    /// name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to divide.</param>
    /// <param name="divisor">The factor by which to divide the formula. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula Divide<T>(Formula value, T divisor) where T : IFloatingPoint<T>
    {
        if (divisor <= T.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(divisor));
        }
        var combined = new Dictionary<string, ushort>(value._isotopes);
        foreach (var isotope in combined.ToList())
        {
            var isotopeValue = T.CreateSaturating(isotope.Value);
            var amount = (isotopeValue / divisor).RoundToInt();
            if (amount == 0)
            {
                combined.Remove(isotope.Key);
            }
            else
            {
                combined[isotope.Key] = (ushort)amount;
            }
        }
        var valueCharge = T.CreateSaturating(value.Charge);
        return new Formula((short)(valueCharge / divisor).RoundToInt(), combined);
    }

    /// <summary>
    /// Get the given formula, with all its nuclide counts multiplied by the given <paramref
    /// name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to multiply.</param>
    /// <param name="factor">The factor by which to multiply the formula. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula Multiply(Formula value, decimal factor)
    {
        if (factor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(factor));
        }
        var combined = new Dictionary<string, ushort>(value._isotopes);
        foreach (var isotope in combined.ToList())
        {
            var amount = (int)Math.Round(isotope.Value * factor);
            if (amount == 0)
            {
                combined.Remove(isotope.Key);
            }
            else
            {
                combined[isotope.Key] = (ushort)amount;
            }
        }
        return new Formula((short)Math.Round(value.Charge * factor), combined);
    }

    /// <summary>
    /// Get the given formula, with all its nuclide counts multiplied by the given <paramref
    /// name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to multiply.</param>
    /// <param name="factor">The factor by which to multiply the formula. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula Multiply(Formula value, double factor)
    {
        if (factor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(factor));
        }
        var combined = new Dictionary<string, ushort>(value._isotopes);
        foreach (var isotope in combined.ToList())
        {
            var amount = (int)Math.Round(isotope.Value * factor);
            if (amount == 0)
            {
                combined.Remove(isotope.Key);
            }
            else
            {
                combined[isotope.Key] = (ushort)amount;
            }
        }
        return new Formula((short)Math.Round(value.Charge * factor), combined);
    }

    /// <summary>
    /// Get the given formula, with all its nuclide counts multiplied by the given <paramref
    /// name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to multiply.</param>
    /// <param name="factor">The factor by which to multiply the formula. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula Multiply(Formula value, float factor)
    {
        if (factor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(factor));
        }
        var combined = new Dictionary<string, ushort>(value._isotopes);
        foreach (var isotope in combined.ToList())
        {
            var amount = (int)Math.Round(isotope.Value * factor);
            if (amount == 0)
            {
                combined.Remove(isotope.Key);
            }
            else
            {
                combined[isotope.Key] = (ushort)amount;
            }
        }
        return new Formula((short)Math.Round(value.Charge * factor), combined);
    }

    /// <summary>
    /// Get the given formula, with all its nuclide counts multiplied by the given <paramref
    /// name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to multiply.</param>
    /// <param name="factor">The factor by which to multiply the formula. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula Multiply<T>(Formula value, T factor) where T : IFloatingPoint<T>
    {
        if (factor < T.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(factor));
        }
        var combined = new Dictionary<string, ushort>(value._isotopes);
        foreach (var isotope in combined.ToList())
        {
            var isotopeValue = T.CreateSaturating(isotope.Value);
            var amount = (isotopeValue * factor).RoundToInt();
            if (amount == 0)
            {
                combined.Remove(isotope.Key);
            }
            else
            {
                combined[isotope.Key] = (ushort)amount;
            }
        }
        var valueCharge = T.CreateSaturating(value.Charge);
        return new Formula((short)(valueCharge * factor).RoundToInt(), combined);
    }

    /// <summary>
    /// Parses the given chemical <paramref name="formula"/>.
    /// </summary>
    /// <param name="formula">
    /// <para>
    /// A chemical formula.
    /// </para>
    /// <para>
    /// Element symbols are case sensitive.
    /// </para>
    /// <para>
    /// Elements may appear any number of times, and the amounts for each instance will be
    /// summed. No chemical analysis or logic is performed which would reject an "impossible" or
    /// improperly written formula.
    /// </para>
    /// <para>
    /// Isotopes other than the most common may be indicated by placing the number in curly
    /// braces before the symbol: e.g. {2}H would indicate deuterium. Atomic mass numbers which
    /// are actually in superscript notation are also recognized, both unicode and the HTML
    /// &lt;sup&gt; tag.
    /// </para>
    /// <para>
    /// Element numbers may appear in normal case following the symbol (e.g. H2O). Actual
    /// subscript is also recognized, both unicode or the HTML &lt;sub&gt; tag.
    /// </para>
    /// <para>
    /// A number which occurs at the beginning of a formula, or following a period, or in a
    /// position where no other number is expected (such as following a space, and not
    /// surrounded by brackets which would indicate that it is an atomic mass number), the
    /// number is taken as a multiplier for the entire group which follows.
    /// </para>
    /// <para>
    /// Parenthesis and square brackets are recognized, and the number immediately following a
    /// given pair (if any) multiplies the values within, as should be expected. For example,
    /// (HO)2 would parse the same as H2O2.
    /// </para>
    /// <para>
    /// A positive or negative sign followed by a number, whether normal or superscript, is
    /// taken to indicate that the formula as a whole is an ion. If a positive or negative sign
    /// appears more than once, the last such sign and its corresponding number take precedence.
    /// </para>
    /// <para>
    /// All other punctuation is ignored. For example: Na,Cl would be interpreted as NaCl (with
    /// no comma).
    /// </para>
    /// </param>
    /// <returns>The resulting formula.</returns>
    /// <exception cref="ArgumentException"><paramref name="formula"/> could not be successfully
    /// parsed.</exception>
    public static Formula Parse(ReadOnlySpan<char> formula)
    {
        if (!TryParse(formula, out var result))
        {
            throw new ArgumentException($"{nameof(formula)} could not be parsed.", nameof(formula));
        }
        return result;
    }

    /// <summary>
    /// Parses the given chemical <paramref name="formula"/>.
    /// </summary>
    /// <param name="formula">
    /// <para>
    /// A chemical formula.
    /// </para>
    /// <para>
    /// Element symbols are case sensitive.
    /// </para>
    /// <para>
    /// Elements may appear any number of times, and the amounts for each instance will be
    /// summed. No chemical analysis or logic is performed which would reject an "impossible" or
    /// improperly written formula.
    /// </para>
    /// <para>
    /// Isotopes other than the most common may be indicated by placing the number in curly
    /// braces before the symbol: e.g. {2}H would indicate deuterium. Atomic mass numbers which
    /// are actually in superscript notation are also recognized, both unicode and the HTML
    /// &lt;sup&gt; tag.
    /// </para>
    /// <para>
    /// Element numbers may appear in normal case following the symbol (e.g. H2O). Actual
    /// subscript is also recognized, both unicode or the HTML &lt;sub&gt; tag.
    /// </para>
    /// <para>
    /// A number which occurs at the beginning of a formula, or following a period, or in a
    /// position where no other number is expected (such as following a space, and not
    /// surrounded by brackets which would indicate that it is an atomic mass number), the
    /// number is taken as a multiplier for the entire group which follows.
    /// </para>
    /// <para>
    /// Parenthesis and square brackets are recognized, and the number immediately following a
    /// given pair (if any) multiplies the values within, as should be expected. For example,
    /// (HO)2 would parse the same as H2O2.
    /// </para>
    /// <para>
    /// A positive or negative sign followed by a number, whether normal or superscript, is
    /// taken to indicate that the formula as a whole is an ion. If a positive or negative sign
    /// appears more than once, the last such sign and its corresponding number take precedence.
    /// </para>
    /// <para>
    /// All other punctuation is ignored. For example: Na,Cl would be interpreted as NaCl (with
    /// no comma).
    /// </para>
    /// </param>
    /// <returns>The resulting formula.</returns>
    /// <exception cref="ArgumentException"><paramref name="formula"/> could not be successfully
    /// parsed.</exception>
    public static Formula Parse(string? formula)
    {
        if (!TryParse(formula, out var result))
        {
            throw new ArgumentException($"{nameof(formula)} could not be parsed.", nameof(formula));
        }
        return result;
    }

    /// <summary>
    /// <para>
    /// Subtracts one formula from another.
    /// </para>
    /// <para>
    /// Performs simple subtraction of the numbers of nuclides, removing any which are reduced
    /// to zero or less. No chemical analysis or checking is performed to validate that the
    /// combination is possible. Does not represent a chemical equation.
    /// </para>
    /// </summary>
    /// <param name="first">The formula from which to remove <paramref name="second"/>.</param>
    /// <param name="second">The formula to remove from <paramref name="first"/>.</param>
    /// <returns>The formula which represents the difference between <paramref name="first"/>
    /// and <paramref name="second"/>.</returns>
    /// <exception cref="OverflowException">The combined charges of the formulas exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    /// <remarks>
    /// <para>
    /// Note that when subtracting ions, the resulting charge may be incorrect if the final
    /// amount of any nuclide was truncated to zero rather than becoming negative (since the
    /// simple subtraction of charge values may incorrectly reflect the actual balance of
    /// electrons with some nuclides having been removed from the system). It is up to calling
    /// code to verify that a subtraction is valid before accepting the accuracy of any result.
    /// </para>
    /// <para>
    /// One way to perform this verification is to call <see cref="Contains(Formula)"/>, which
    /// will verify that the subtraction will not result in any truncation.
    /// </para>
    /// </remarks>
    public static Formula Subtract(Formula first, Formula second)
    {
        var combinedIsotopes = new Dictionary<string, ushort>(first._isotopes);
        foreach (var isotope in second._isotopes.Where(x => combinedIsotopes.ContainsKey(x.Key)).ToList())
        {
            var value = combinedIsotopes[isotope.Key] - isotope.Value;
            if (value <= 0)
            {
                combinedIsotopes.Remove(isotope.Key);
            }
            else
            {
                combinedIsotopes[isotope.Key] = (ushort)value;
            }
        }
        return new Formula((short)(first.Charge - second.Charge), combinedIsotopes);
    }

    /// <summary>
    /// Attempts to parse the given chemical <paramref name="formula"/>.
    /// </summary>
    /// <param name="formula">
    /// <para>
    /// A chemical formula.
    /// </para>
    /// <para>
    /// Element symbols are case sensitive.
    /// </para>
    /// <para>
    /// Elements may appear any number of times, and the amounts for each instance will be
    /// summed. No chemical analysis or logic is performed which would reject an "impossible" or
    /// improperly written formula.
    /// </para>
    /// <para>
    /// Isotopes other than the most common may be indicated by placing the number in curly
    /// braces before the symbol: e.g. {2}H would indicate deuterium. Atomic mass numbers which
    /// are actually in superscript notation are also recognized, both unicode and the HTML
    /// &lt;sup&gt; tag.
    /// </para>
    /// <para>
    /// Element numbers may appear in normal case following the symbol (e.g. H2O). Actual
    /// subscript is also recognized, both unicode or the HTML &lt;sub&gt; tag.
    /// </para>
    /// <para>
    /// A number which occurs at the beginning of a formula, or following a period, or in a
    /// position where no other number is expected (such as following a space, and not
    /// surrounded by brackets which would indicate that it is an atomic mass number), the
    /// number is taken as a multiplier for the entire group which follows.
    /// </para>
    /// <para>
    /// Parenthesis and square brackets are recognized, and the number immediately following a
    /// given pair (if any) multiplies the values within, as should be expected. For example,
    /// (HO)2 would parse the same as H2O2.
    /// </para>
    /// <para>
    /// A positive or negative sign followed by a number, whether normal or superscript, is
    /// taken to indicate that the formula as a whole is an ion. If a positive or negative sign
    /// appears more than once, the last such sign and its corresponding number take precedence.
    /// </para>
    /// <para>
    /// All other punctuation is ignored. For example: Na,Cl would be interpreted as NaCl (with
    /// no comma).
    /// </para>
    /// </param>
    /// <param name="result">If the <paramref name="formula"/> is successfully parsed, will
    /// contain the corresponding <see cref="Formula"/>.</param>
    /// <returns><see langword="true"/> if the formula was successfully parsed; otherwise <see
    /// langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> formula, out Formula result)
    {
        if (formula.Equals("<empty>".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            result = Empty;
            return true;
        }
        var isotopes = new Dictionary<string, ushort>();
        var index = 0;
        if (Parse(formula, isotopes, ref index, out var charge))
        {
            result = new Formula(charge ?? 0, isotopes);
            return true;
        }
        else
        {
            result = new Formula();
            return false;
        }
    }

    /// <summary>
    /// Attempts to parse the given chemical <paramref name="formula"/>.
    /// </summary>
    /// <param name="formula">
    /// <para>
    /// A chemical formula.
    /// </para>
    /// <para>
    /// Element symbols are case sensitive.
    /// </para>
    /// <para>
    /// Elements may appear any number of times, and the amounts for each instance will be
    /// summed. No chemical analysis or logic is performed which would reject an "impossible" or
    /// improperly written formula.
    /// </para>
    /// <para>
    /// Isotopes other than the most common may be indicated by placing the number in curly
    /// braces before the symbol: e.g. {2}H would indicate deuterium. Atomic mass numbers which
    /// are actually in superscript notation are also recognized, both unicode and the HTML
    /// &lt;sup&gt; tag.
    /// </para>
    /// <para>
    /// Element numbers may appear in normal case following the symbol (e.g. H2O). Actual
    /// subscript is also recognized, both unicode or the HTML &lt;sub&gt; tag.
    /// </para>
    /// <para>
    /// A number which occurs at the beginning of a formula, or following a period, or in a
    /// position where no other number is expected (such as following a space, and not
    /// surrounded by brackets which would indicate that it is an atomic mass number), the
    /// number is taken as a multiplier for the entire group which follows.
    /// </para>
    /// <para>
    /// Parenthesis and square brackets are recognized, and the number immediately following a
    /// given pair (if any) multiplies the values within, as should be expected. For example,
    /// (HO)2 would parse the same as H2O2.
    /// </para>
    /// <para>
    /// A positive or negative sign followed by a number, whether normal or superscript, is
    /// taken to indicate that the formula as a whole is an ion. If a positive or negative sign
    /// appears more than once, the last such sign and its corresponding number take precedence.
    /// </para>
    /// <para>
    /// All other punctuation is ignored. For example: Na,Cl would be interpreted as NaCl (with
    /// no comma).
    /// </para>
    /// </param>
    /// <param name="result">If the <paramref name="formula"/> is successfully parsed, will
    /// contain the corresponding <see cref="Formula"/>.</param>
    /// <returns><see langword="true"/> if the formula was successfully parsed; otherwise <see
    /// langword="false"/>.</returns>
    public static bool TryParse(string? formula, out Formula result)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            result = new Formula();
            return false;
        }
        if (formula.Equals("<empty>", StringComparison.OrdinalIgnoreCase))
        {
            result = Empty;
            return true;
        }

        return TryParse(formula.AsSpan(), out result);
    }

    /// <summary>
    /// Adds another formula to this instance.
    /// </summary>
    /// <param name="other">The formula to combine with this instance.</param>
    /// <returns>A combined formula.</returns>
    /// <exception cref="OverflowException">The combined charges of the formulas exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public Formula Add(Formula other) => Add(this, other);

    /// <summary>
    /// Gets a formula that combines this instance with the given <paramref name="isotope"/>.
    /// </summary>
    /// <param name="isotope">The isotope to add to this formula.</param>
    /// <param name="amount">The amount of the given <paramref name="isotope"/> to add.</param>
    /// <returns>A combined formula.</returns>
    public Formula Add(Isotope isotope, ushort amount = 1)
    {
        var combined = new Dictionary<string, ushort>(_isotopes);
        var key = IsotopeConverter.ConvertToString(isotope);
        if (combined.ContainsKey(key))
        {
            combined[key] += amount;
        }
        else
        {
            combined[key] = amount;
        }
        return new Formula(Charge, combined);
    }

    /// <summary>
    /// Get a formula which has all this instance's nuclide counts divided by the given
    /// <paramref name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="divisor">The factor by which to divide this instance. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public Formula Divide(decimal divisor) => Divide(this, divisor);

    /// <summary>
    /// Get a formula which has all this instance's nuclide counts divided by the given
    /// <paramref name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="divisor">The factor by which to divide this instance. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public Formula Divide(double divisor) => Divide(this, divisor);

    /// <summary>
    /// Get a formula which has all this instance's nuclide counts divided by the given
    /// <paramref name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="divisor">The factor by which to divide this instance. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public Formula Divide(float divisor) => Divide(this, divisor);

    /// <summary>
    /// Get a formula which has all this instance's nuclide counts divided by the given
    /// <paramref name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="divisor">The factor by which to divide this instance. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public Formula Divide<T>(T divisor) where T : IFloatingPoint<T> => Divide(this, divisor);

    /// <summary>
    /// Determines whether this instance contains all the nuclides in the <paramref
    /// name="other"/> formula in equal or greater amounts.
    /// </summary>
    /// <param name="other">A formula to compare with this instance.</param>
    /// <returns><see langword="true"/> if this instance contains all the nuclides in <paramref
    /// name="other"/> in equal or greater amounts; otherwise <see langword="false"/>.</returns>
    public bool Contains(Formula other)
    {
        var isotopes = _isotopes;
        return !other._isotopes.Any(x => !isotopes.TryGetValue(x.Key, out var value) || value < x.Value);
    }

    /// <summary>
    /// Determines whether this instance contains the given <paramref name="isotope"/>.
    /// </summary>
    /// <param name="isotope">An isotope to check for includion in this instance.</param>
    /// <returns><see langword="true"/> if this instance contains the given <paramref
    /// name="isotope"/>; otherwise <see langword="false"/>.</returns>
    public bool Contains(Isotope isotope) => _isotopes.ContainsKey(IsotopeConverter.ConvertToString(isotope));

    /// <summary>
    /// Determines whether this instance contains any isotopes of the given <paramref
    /// name="element"/>.
    /// </summary>
    /// <param name="element">An element to check for includion in this instance.</param>
    /// <returns><see langword="true"/> if this instance contains the given <paramref
    /// name="element"/>; otherwise <see langword="false"/>.</returns>
    public bool Contains(Element element) => Elements.Any(x => x.Equals(element));

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Formula? other)
        => other.HasValue
        && EqualityComparer<Dictionary<string, ushort>>.Default.Equals(_isotopes, other.Value._isotopes)
        && Charge == other.Value.Charge;

    /// <summary>Indicates whether the current object is equal to another object of the same
    /// type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals([AllowNull] Formula other)
        => _isotopes.OrderBy(x => x.Key).SequenceEqual(other._isotopes.OrderBy(y => y.Key))
        && Charge == other.Charge;

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance
    /// are the same type and represent the same value; otherwise, <see
    /// langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Formula formula && Equals(formula);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
        var hashCode = -2078353425;
        hashCode = (hashCode * -1521134295) + EqualityComparer<Dictionary<string, ushort>>.Default.GetHashCode(_isotopes);
        return (hashCode * -1521134295) + Charge.GetHashCode();
    }

    /// <summary>
    /// Get a formula which has all this instance's nuclide counts multiplied by the given
    /// <paramref name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="factor">The factor by which to multiply this instance. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public Formula Multiply(decimal factor) => Multiply(this, factor);

    /// <summary>
    /// Get a formula which has all this instance's nuclide counts multiplied by the given
    /// <paramref name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="factor">The factor by which to multiply this instance. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public Formula Multiply(double factor) => Multiply(this, factor);

    /// <summary>
    /// Get a formula which has all this instance's nuclide counts multiplied by the given
    /// <paramref name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="factor">The factor by which to multiply this instance. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public Formula Multiply(float factor) => Multiply(this, factor);

    /// <summary>
    /// Get a formula which has all this instance's nuclide counts multiplied by the given
    /// <paramref name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="factor">The factor by which to multiply this instance. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public Formula Multiply<T>(T factor) where T : IFloatingPoint<T> => Multiply(this, factor);

    /// <summary>
    /// <para>
    /// Gets a formula that removes all of the given <paramref name="isotope"/> from this
    /// formula.
    /// </para>
    /// <para>
    /// If this formula does not contain the given <paramref name="isotope"/>, no exception will
    /// occur.
    /// </para>
    /// </summary>
    /// <param name="isotope">The isotope to remove from this formula.</param>
    /// <returns>The resulting formula.</returns>
    public Formula Remove(Isotope isotope)
    {
        var combined = new Dictionary<string, ushort>(_isotopes);
        combined.Remove(IsotopeConverter.ConvertToString(isotope));
        return new Formula(Charge, combined);
    }

    /// <summary>
    /// <para>
    /// Gets a formula that removes all isotopes of the given <paramref name="element"/> from this
    /// formula.
    /// </para>
    /// <para>
    /// If this formula does not contain the given <paramref name="element"/>, no exception will
    /// occur.
    /// </para>
    /// </summary>
    /// <param name="element">The element to remove from this formula.</param>
    /// <returns>The resulting formula.</returns>
    public Formula Remove(Element element)
    {
        var combined = new Dictionary<string, ushort>();
        foreach (var item in _isotopes.Where(x => !PeriodicTable.TryGetIsotope(x.Key, out var isotope) || !isotope.Element.Equals(element)))
        {
            combined.Add(item.Key, item.Value);
        }
        return new Formula(Charge, combined);
    }

    /// <summary>
    /// <para>
    /// Gets a formula that removes all of the given <paramref name="isotopes"/> from this
    /// formula.
    /// </para>
    /// <para>
    /// If this formula does not contain any of the given <paramref name="isotopes"/>, no exception will
    /// occur.
    /// </para>
    /// </summary>
    /// <param name="isotopes">The isotopes to remove from this formula.</param>
    /// <returns>The resulting formula.</returns>
    public Formula RemoveAll(IEnumerable<Isotope> isotopes)
    {
        var combined = new Dictionary<string, ushort>(_isotopes);
        foreach (var isotope in isotopes)
        {
            combined.Remove(IsotopeConverter.ConvertToString(isotope));
        }
        return new Formula(Charge, combined);
    }

    /// <summary>
    /// <para>
    /// Gets a formula that removes all of the given <paramref name="isotopes"/> from this
    /// formula.
    /// </para>
    /// <para>
    /// If this formula does not contain any of the given <paramref name="isotopes"/>, no exception will
    /// occur.
    /// </para>
    /// </summary>
    /// <param name="isotopes">The isotopes to remove from this formula.</param>
    /// <returns>The resulting formula.</returns>
    public Formula RemoveAll(params Isotope[] isotopes)
    {
        var combined = new Dictionary<string, ushort>(_isotopes);
        if (isotopes is not null)
        {
            foreach (var isotope in isotopes)
            {
                combined.Remove(IsotopeConverter.ConvertToString(isotope));
            }
        }
        return new Formula(Charge, combined);
    }

    /// <summary>
    /// <para>
    /// Gets a formula that removes all isotopes of the given <paramref name="elements"/> from
    /// this formula.
    /// </para>
    /// <para>
    /// If this formula does not contain any of the given <paramref name="elements"/>, no
    /// exception will occur.
    /// </para>
    /// </summary>
    /// <param name="elements">The elements to remove from this formula.</param>
    /// <returns>The resulting formula.</returns>
    public Formula RemoveAll(IEnumerable<Element> elements)
    {
        var combined = new Dictionary<string, ushort>();
        foreach (var item in _isotopes.Where(x => !PeriodicTable.TryGetIsotope(x.Key, out var isotope) || !elements.Contains(isotope.Element)))
        {
            combined.Add(item.Key, item.Value);
        }
        return new Formula(Charge, combined);
    }

    /// <summary>
    /// <para>
    /// Gets a formula that removes all isotopes of the given <paramref name="elements"/> from
    /// this formula.
    /// </para>
    /// <para>
    /// If this formula does not contain any of the given <paramref name="elements"/>, no
    /// exception will occur.
    /// </para>
    /// </summary>
    /// <param name="elements">The elements to remove from this formula.</param>
    /// <returns>The resulting formula.</returns>
    public Formula RemoveAll(params Element[] elements)
    {
        var combined = new Dictionary<string, ushort>();
        if (elements is not null)
        {
            foreach (var item in _isotopes.Where(x => !PeriodicTable.TryGetIsotope(x.Key, out var isotope) || !elements.Contains(isotope.Element)))
            {
                combined.Add(item.Key, item.Value);
            }
        }
        return new Formula(Charge, combined);
    }

    /// <summary>
    /// <para>
    /// Subtracts the given formula from this instance.
    /// </para>
    /// <para>
    /// Performs simple subtraction of the numbers of nuclides, removing any which are reduced
    /// to zero or less. No chemical analysis or checking is performed to validate that the
    /// combination is possible. Does not represent a chemical equation.
    /// </para>
    /// </summary>
    /// <param name="other">The formula to remove from this instance.</param>
    /// <returns>The formula which represents the difference between this instance
    /// and <paramref name="other"/>.</returns>
    /// <exception cref="OverflowException">The combined charges of the formulas exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    /// <remarks>
    /// <para>
    /// Note that when subtracting ions, the resulting charge may be incorrect if the final
    /// amount of any nuclide was truncated to zero rather than becoming negative (since the
    /// simple subtraction of charge values may incorrectly reflect the actual balance of
    /// electrons with some nuclides having been removed from the system). It is up to calling
    /// code to verify that a subtraction is valid before accepting the accuracy of any result.
    /// </para>
    /// <para>
    /// One way to perform this verification is to call <see cref="Contains(Formula)"/>, which
    /// will verify that the subtraction will not result in any truncation.
    /// </para>
    /// </remarks>
    public Formula Subtract(Formula other) => Subtract(this, other);

    /// <summary>
    /// <para>
    /// Gets a formula that removes the given <paramref name="isotope"/> from this formula.
    /// </para>
    /// <para>
    /// If this formula does not contain the given <paramref name="isotope"/>, or the given
    /// <paramref name="amount"/> of it, no exception will occur.
    /// </para>
    /// </summary>
    /// <param name="isotope">The isotope to remove from this formula.</param>
    /// <param name="amount">The amount of the given <paramref name="isotope"/> to remove.</param>
    /// <returns>The resulting formula.</returns>
    public Formula Subtract(Isotope isotope, ushort amount = 1)
    {
        var combined = new Dictionary<string, ushort>(_isotopes);
        var key = IsotopeConverter.ConvertToString(isotope);
        if (combined.TryGetValue(key, out var value))
        {
            var newValue = value - amount;
            if (newValue <= 0)
            {
                combined.Remove(key);
            }
            else
            {
                combined[key] = (ushort)newValue;
            }
        }
        return new Formula(Charge, combined);
    }

    /// <summary>Returns the string equivalent of this formula in Hill notation.</summary>
    /// <returns>A string equivalent of this formula in Hill notation.</returns>
    public override string ToString()
    {
        if (_isotopes is null)
        {
            return "<empty>";
        }

        var sb = new StringBuilder();

        var isotopes = _isotopes.Select(x => (isotope: PeriodicTable.TryGetIsotope(x.Key, out var isotope) ? isotope : Isotope.Empty, number: x.Value)).ToList();
        // carbons first
        foreach (var isotope in isotopes
            .Where(x => x.isotope.AtomicNumber == 6)
            .OrderByDescending(x => x.isotope.MassNumber))
        {
            sb.Append(isotope.isotope.ToString());
            if (isotope.number > 1)
            {
                sb.Append(isotope.number.ToSubscript());
            }
        }
        var carbons = sb.Length > 0;
        // if carbon, hydrogens second
        if (carbons)
        {
            foreach (var isotope in isotopes
                .Where(x => x.isotope.AtomicNumber == 1)
                .OrderByDescending(x => x.isotope.MassNumber))
            {
                sb.Append(isotope.isotope.ToString());
                if (isotope.number > 1)
                {
                    sb.Append(isotope.number.ToSubscript());
                }
            }
        }
        // all except carbons; and if there were carbons, except hydrogens as well
        foreach (var isotope in isotopes
            .Where(x => !carbons || (x.isotope.AtomicNumber != 1 && x.isotope.AtomicNumber != 6))
            .OrderBy(x => x.isotope.Symbol)
            .ThenByDescending(x => x.isotope.MassNumber))
        {
            sb.Append(isotope.isotope.ToString());
            if (isotope.number > 1)
            {
                sb.Append(isotope.number.ToSubscript());
            }
        }
        if (Charge != 0)
        {
            sb.Append(Charge.ToSuperscript("N0", positiveSign: true, postfixSign: true));
        }
        if (sb.Length == 0)
        {
            sb.Append("<empty>");
        }
        return sb.ToString();
    }

    private static bool Parse(ReadOnlySpan<char> formula, Dictionary<string, ushort> isotopes, ref int index, out short? charge)
    {
        Isotope? isotope = null;
        ushort? massNumber = null;
        charge = null;
        var startGroup = true;
        int? multiplier = null;
        while (formula.Length > index)
        {
            if (startGroup)
            {
                var startIndex = index;
                if (ParseNumber(formula, ref index, out var multiplierValue, out var sub, out var sup, out var ion))
                {
                    if (sub || sup || ion)
                    {
                        index = startIndex;
                    }
                    else if (multiplier > ushort.MaxValue)
                    {
                        return false;
                    }
                    else
                    {
                        multiplier = multiplierValue;
                    }
                }
            }
            startGroup = false;
            switch (formula[index])
            {
                case '.':
                    multiplier = null;
                    startGroup = true;
                    index++;
                    continue;
                case '(':
                    if (isotope.HasValue)
                    {
                        var key = IsotopeConverter.ConvertToString(isotope.Value);
                        if (isotopes.TryGetValue(key, out var value))
                        {
                            var v = value + (ushort)(multiplier ?? 1);
                            if (v > ushort.MaxValue)
                            {
                                return false;
                            }
                            else
                            {
                                isotopes[key] = (ushort)v;
                            }
                        }
                        else
                        {
                            isotopes[key] = (ushort)(multiplier ?? 1);
                        }
                    }
                    if (massNumber.HasValue
                        || !ParseGroup(formula, isotopes, ref index, multiplier, ')', out charge))
                    {
                        return false;
                    }
                    continue;
                case '[':
                    if (isotope.HasValue)
                    {
                        var key = IsotopeConverter.ConvertToString(isotope.Value);
                        if (isotopes.TryGetValue(key, out var value))
                        {
                            var v = value + (ushort)(multiplier ?? 1);
                            if (v > ushort.MaxValue)
                            {
                                return false;
                            }
                            else
                            {
                                isotopes[key] = (ushort)v;
                            }
                        }
                        else
                        {
                            isotopes[key] = (ushort)(multiplier ?? 1);
                        }
                    }
                    if (massNumber.HasValue
                        || !ParseGroup(formula, isotopes, ref index, multiplier, ']', out charge))
                    {
                        return false;
                    }
                    continue;
                case '{':
                    if (isotope.HasValue)
                    {
                        var key = IsotopeConverter.ConvertToString(isotope.Value);
                        if (isotopes.TryGetValue(key, out var value))
                        {
                            var v = value + (ushort)(multiplier ?? 1);
                            if (v > ushort.MaxValue)
                            {
                                return false;
                            }
                            else
                            {
                                isotopes[key] = (ushort)v;
                            }
                        }
                        else
                        {
                            isotopes[key] = (ushort)(multiplier ?? 1);
                        }
                    }
                    if (massNumber.HasValue
                        || !ParseMassNumber(formula, ref index, out var massNumberValue))
                    {
                        return false;
                    }
                    else if (massNumberValue is < ushort.MinValue or > ushort.MaxValue)
                    {
                        return false;
                    }
                    else
                    {
                        massNumber = (ushort)massNumberValue;
                    }
                    continue;
            }
            if (ParseNumber(formula, ref index, out var number, out _, out var superscript, out var ionCharge))
            {
                if (massNumber.HasValue)
                {
                    return false;
                }
                else if (superscript || ionCharge)
                {
                    if (isotope.HasValue || ionCharge)
                    {
                        if (formula.Length <= index)
                        {
                            if (number is < short.MinValue or > short.MaxValue)
                            {
                                return false;
                            }
                            else
                            {
                                charge = (short)number;
                            }
                        }
                        else if (ionCharge || formula[index] == '\u207A')
                        {
                            if (number is < short.MinValue or > short.MaxValue)
                            {
                                return false;
                            }
                            else
                            {
                                charge = (short)number;
                            }
                            index++;
                        }
                        else if (formula[index] == '\u207B')
                        {
                            number = -number;
                            if (number is < short.MinValue or > short.MaxValue)
                            {
                                return false;
                            }
                            else
                            {
                                charge = (short)number;
                            }
                            index++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (number is < ushort.MinValue or > ushort.MaxValue)
                    {
                        return false;
                    }
                    else
                    {
                        massNumber = (ushort)number;
                    }
                    continue;
                }
                else if (isotope.HasValue)
                {
                    if (number < 0)
                    {
                        return false;
                    }
                    else if (number > 0)
                    {
                        number *= multiplier ?? 1;
                        var key = IsotopeConverter.ConvertToString(isotope.Value);
                        if (number > ushort.MaxValue)
                        {
                            return false;
                        }
                        else if (isotopes.ContainsKey(key))
                        {
                            isotopes[key] += (ushort)number;
                        }
                        else
                        {
                            isotopes[key] = (ushort)number;
                        }
                    }
                    isotope = null;
                    continue;
                }
                else if (number > ushort.MaxValue)
                {
                    return false;
                }
                else
                {
                    multiplier = number;
                }
            }
            if (ParseSymbol(formula, ref index, out var element))
            {
                if (isotope.HasValue)
                {
                    var key = IsotopeConverter.ConvertToString(isotope.Value);
                    if (isotopes.TryGetValue(key, out var value))
                    {
                        var v = value + (ushort)(multiplier ?? 1);
                        if (v > ushort.MaxValue)
                        {
                            return false;
                        }
                        else
                        {
                            isotopes[key] = (ushort)v;
                        }
                    }
                    else
                    {
                        isotopes[key] = (ushort)(multiplier ?? 1);
                    }
                }
                if (massNumber.HasValue)
                {
                    if (!PeriodicTable.Instance[element].ContainsKey(massNumber.Value))
                    {
                        return false;
                    }
                    else
                    {
                        isotope = PeriodicTable.Instance[element][massNumber.Value];
                        massNumber = null;
                    }
                }
                else
                {
                    if (!PeriodicTable.TryGetCommonIsotope(element, out var isotopeValue))
                    {
                        return false;
                    }
                    else
                    {
                        isotope = isotopeValue;
                    }
                }
                continue;
            }
            if (isotope.HasValue)
            {
                var key = IsotopeConverter.ConvertToString(isotope.Value);
                if (isotopes.TryGetValue(key, out var value))
                {
                    var v = value + (ushort)(multiplier ?? 1);
                    if (v > ushort.MaxValue)
                    {
                        return false;
                    }
                    else
                    {
                        isotopes[key] = (ushort)v;
                    }
                }
                else
                {
                    isotopes[key] = (ushort)(multiplier ?? 1);
                }
            }
            index++;
        }
        if (isotope.HasValue)
        {
            var key = IsotopeConverter.ConvertToString(isotope.Value);
            if (isotopes.TryGetValue(key, out var value))
            {
                var v = value + (ushort)(multiplier ?? 1);
                if (v > ushort.MaxValue)
                {
                    return false;
                }
                else
                {
                    isotopes[key] = (ushort)v;
                }
            }
            else
            {
                isotopes[key] = (ushort)(multiplier ?? 1);
            }
        }
        return true;
    }

    private static bool ParseGroup(ReadOnlySpan<char> formula, Dictionary<string, ushort> isotopes, ref int index, int? multiplier, char closer, out short? charge)
    {
        index++;
        charge = null;

        var indexCloser = formula.LastIndexOf(closer);
        if (indexCloser == -1 || indexCloser < index)
        {
            return false;
        }

        var group = new Dictionary<string, ushort>();
        var sliceIndex = 0;
        if (Parse(formula[index..indexCloser], group, ref sliceIndex, out charge))
        {
            index += sliceIndex + 1;
            var previousIndex = index;
            if (!ParseNumber(formula, ref index, out var groupMultiplier, out _, out var superscript, out var groupCharge))
            {
                groupMultiplier = 1;
            }
            else if (groupCharge)
            {
                if (groupMultiplier is < short.MinValue or > short.MaxValue)
                {
                    return false;
                }
                charge = (short)groupMultiplier;
                groupMultiplier = 1;
            }
            else if (superscript)
            {
                index = previousIndex;
                groupMultiplier = 1;
            }
            foreach (var item in group)
            {
                var value = item.Value * groupMultiplier * (multiplier ?? 1);
                if (value > ushort.MaxValue)
                {
                    return false;
                }
                else if (isotopes.TryGetValue(item.Key, out var isotopeValue))
                {
                    value += isotopeValue;
                    if (value > ushort.MaxValue)
                    {
                        return false;
                    }
                    else
                    {
                        isotopes[item.Key] += (ushort)value;
                    }
                }
                else
                {
                    isotopes[item.Key] = (ushort)value;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool ParseMassNumber(ReadOnlySpan<char> formula, ref int index, out int massNumber)
    {
        index++;

        if (!ParseNumber(formula, ref index, out massNumber, out _, out _, out _))
        {
            return false;
        }

        if (formula.Length <= index || formula[index] != '}')
        {
            return false;
        }

        index++;
        return true;
    }

    private static bool ParseNumber(ReadOnlySpan<char> formula, ref int index, out int value, out bool subscript, out bool superscript, out bool charge)
    {
        var start = index;
        subscript = false;
        superscript = false;
        charge = false;
        var isNegative = false;
        var leadingSign = false;
        value = 0;
        if (formula.Length > index)
        {
            if (string.Equals(formula.Slice(index, CultureInfo.CurrentCulture.NumberFormat.PositiveSign.Length).ToString(), CultureInfo.CurrentCulture.NumberFormat.PositiveSign, StringComparison.CurrentCulture))
            {
                charge = true;
                index++;
            }
            else if (formula[index] == '\u207A')
            {
                leadingSign = true;
                superscript = true;
                index++;
            }
            else if (string.Equals(formula.Slice(index, CultureInfo.CurrentCulture.NumberFormat.NegativeSign.Length).ToString(), CultureInfo.CurrentCulture.NumberFormat.NegativeSign, StringComparison.CurrentCulture))
            {
                charge = true;
                isNegative = true;
                index++;
            }
            else if (formula[index] == '\u207B')
            {
                leadingSign = true;
                superscript = true;
                isNegative = true;
                index++;
            }
        }
        while (formula.Length > index
            && formula[index].TryParseDigit(out var v, out var dSuperscript, out var dSubscript)
            && (start == index || dSuperscript == superscript)
            && (start == index || dSubscript == subscript))
        {
            superscript = dSuperscript;
            subscript = dSubscript;
            index++;
            value *= 10;
            value += v;
        }
        if (isNegative)
        {
            value = -value;
        }
        else if (superscript && formula.Length > index && !leadingSign)
        {
            if (formula[index] == '\u207A')
            {
                charge = true;
                index++;
            }
            else if (formula[index] == '\u207B')
            {
                charge = true;
                value = -value;
                index++;
            }
        }
        return index != start;
    }

    private static bool ParseSymbol(ReadOnlySpan<char> formula, ref int index, out Element element)
    {
        if (formula.Length <= index)
        {
            element = PeriodicTable.GetElement(1);
            return false;
        }
        var sb = new StringBuilder();
        if (char.IsUpper(formula[index]))
        {
            sb.Append(formula[index]);
            index++;
        }
        if (index < formula.Length
            && char.IsLower(formula[index]))
        {
            sb.Append(formula[index]);
            index++;
        }
        var symbol = sb.ToString();
        return PeriodicTable.TryGetElement(symbol, out element);
    }

    /// <summary>
    /// <para>
    /// Adds two formulas.
    /// </para>
    /// <para>
    /// Performs simple summation of the numbers of nuclides. No chemical analysis or checking
    /// is performed to validate that the combination is possible. Does not represent a chemical
    /// equation.
    /// </para>
    /// </summary>
    /// <param name="first">The first formula to combine.</param>
    /// <param name="second">The second formula to combine.</param>
    /// <returns>A combined formula.</returns>
    /// <exception cref="OverflowException">The combined charges of the formulas exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula operator +(Formula first, Formula second) => Add(first, second);

    /// <summary>
    /// <para>
    /// Subtracts one formula from another.
    /// </para>
    /// <para>
    /// Performs simple subtraction of the numbers of nuclides, removing any which are reduced
    /// to zero or less. No chemical analysis or checking is performed to validate that the
    /// combination is possible. Does not represent a chemical equation.
    /// </para>
    /// </summary>
    /// <param name="first">The formula from which to remove <paramref name="second"/>.</param>
    /// <param name="second">The formula to remove from <paramref name="first"/>.</param>
    /// <returns>The formula which represents the difference between <paramref name="first"/>
    /// and <paramref name="second"/>.</returns>
    /// <exception cref="OverflowException">The combined charges of the formulas exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    /// <remarks>
    /// <para>
    /// Note that when subtracting ions, the resulting charge may be incorrect if the final
    /// amount of any nuclide was truncated to zero rather than becoming negative (since the
    /// simple subtraction of charge values may incorrectly reflect the actual balance of
    /// electrons with some nuclides having been removed from the system). It is up to calling
    /// code to verify that a subtraction is valid before accepting the accuracy of any result.
    /// </para>
    /// <para>
    /// One way to perform this verification is to call <see cref="Contains(Formula)"/>, which
    /// will verify that the subtraction will not result in any truncation.
    /// </para>
    /// </remarks>
    public static Formula operator -(Formula first, Formula second) => Subtract(first, second);

    /// <summary>
    /// Get the given formula, with all its nuclide counts multiplied by the given <paramref
    /// name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to multiply.</param>
    /// <param name="factor">The factor by which to multiply the formula. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula operator *(Formula value, decimal factor) => Multiply(value, factor);

    /// <summary>
    /// Get the given formula, with all its nuclide counts multiplied by the given <paramref
    /// name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to multiply.</param>
    /// <param name="factor">The factor by which to multiply the formula. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula operator *(Formula value, double factor) => Multiply(value, factor);

    /// <summary>
    /// Get the given formula, with all its nuclide counts multiplied by the given <paramref
    /// name="factor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to multiply.</param>
    /// <param name="factor">The factor by which to multiply the formula. Must be greater than
    /// or equal to zero.</param>
    /// <returns>The multiplied formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than
    /// zero.</exception>
    /// <exception cref="OverflowException">The multiplied charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula operator *(Formula value, float factor) => Multiply(value, factor);

    /// <summary>
    /// Get the given formula, with all its nuclide counts divided by the given <paramref
    /// name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to divide.</param>
    /// <param name="divisor">The factor by which to divide the formula. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula operator /(Formula value, decimal divisor) => Divide(value, divisor);

    /// <summary>
    /// Get the given formula, with all its nuclide counts divided by the given <paramref
    /// name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to divide.</param>
    /// <param name="divisor">The factor by which to divide the formula. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula operator /(Formula value, double divisor) => Divide(value, divisor);

    /// <summary>
    /// Get the given formula, with all its nuclide counts divided by the given <paramref
    /// name="divisor"/>, and rounded to the nearest whole integer.
    /// </summary>
    /// <param name="value">The formula to divide.</param>
    /// <param name="divisor">The factor by which to divide the formula. Must be greater than
    /// zero.</param>
    /// <returns>The divided formula.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is less than
    /// or equal to zero.</exception>
    /// <exception cref="OverflowException">The divided charge of the formula exceeds <see
    /// cref="short.MaxValue"/> or <see cref="short.MinValue"/>, or the amount of any nuclide
    /// exceeds <see cref="ushort.MaxValue"/>.</exception>
    public static Formula operator /(Formula value, float divisor) => Divide(value, divisor);

    /// <summary>
    /// Indicates whether two <see cref="Formula"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(Formula left, Formula right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two <see cref="Formula"/> instances are unequal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are unequal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(Formula left, Formula right) => !(left == right);
}
