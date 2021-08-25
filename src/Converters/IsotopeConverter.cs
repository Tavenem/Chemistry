using System.ComponentModel;
using System.Globalization;

namespace Tavenem.Chemistry.Elements;

/// <summary>
/// Type converter for <see cref="Isotope"/>. Supports string conversion.
/// </summary>
public class IsotopeConverter : TypeConverter
{
    /// <summary>
    /// A static instance of <see cref="IsotopeConverter"/>.
    /// </summary>
    public static readonly IsotopeConverter Instance = new();

    /// <summary>Converts the given string to an <see cref="Isotope"/>.</summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>An <see cref="Isotope" /> that represents the converted value.</returns>
    public new static Isotope ConvertFromString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }
        var span = value.AsSpan();
        var indexOfSep = span.IndexOf(':');
        if (indexOfSep == -1)
        {
            throw new NotSupportedException("Invalid format: no separator");
        }
        if (indexOfSep == 0)
        {
            throw new NotSupportedException("Invalid format: no atomic number");
        }
        if (indexOfSep == span.Length - 1)
        {
            throw new NotSupportedException("Invalid format: no mass number");
        }
        var atomicNumberSlice = span.Slice(0, indexOfSep);
        if (!ushort.TryParse(atomicNumberSlice, out var atomicNumber))
        {
            throw new NotSupportedException("Invalid format: cannot parse atomic number");
        }
        if (!PeriodicTable.TryGetIsotopes(atomicNumber, out var isotopes))
        {
            throw new NotSupportedException("Element with atomic number not found");
        }
        var massNumberSlice = span[(indexOfSep + 1)..];
        if (!ushort.TryParse(massNumberSlice, out var massNumber))
        {
            throw new NotSupportedException("Invalid format: cannot parse mass number");
        }
        if (!isotopes.TryGetValue(massNumber, out var isotope))
        {
            throw new NotSupportedException("Isotope with mass number not found");
        }
        return isotope;
    }

    /// <summary>
    /// Converts the given <see cref="Isotope"/> to a string.
    /// </summary>
    /// <param name="value">The <see cref="Isotope"/> to convert.</param>
    /// <returns>A string that represents the converted value.</returns>
    public static string ConvertToString(Isotope value) => $"{value.AtomicNumber}:{value.MassNumber}";

    /// <summary>Attempts to convert the given string to an <see cref="Isotope"/>.</summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">If successful, will be set to the <see cref="Isotope"/> that
    /// represents the converted value.</param>
    /// <returns><see langword="true"/> if the conversion succeeeds; otherwise <see
    /// langword="false"/>.</returns>
    public static bool TryConvertFromString(string? value, out Isotope result)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = Isotope.Empty;
            return false;
        }
        var span = value.AsSpan();
        var indexOfSep = span.IndexOf(':');
        if (indexOfSep <= 0
            || indexOfSep == span.Length - 1)
        {
            result = Isotope.Empty;
            return false;
        }
        var atomicNumberSlice = span.Slice(0, indexOfSep);
        if (!ushort.TryParse(atomicNumberSlice, out var atomicNumber)
            || !PeriodicTable.TryGetIsotopes(atomicNumber, out var isotopes))
        {
            result = Isotope.Empty;
            return false;
        }
        var massNumberSlice = span[(indexOfSep + 1)..];
        if (!ushort.TryParse(massNumberSlice, out var massNumber))
        {
            result = Isotope.Empty;
            return false;
        }
        return isotopes.TryGetValue(massNumber, out result);
    }

    /// <summary>Returns whether this converter can convert an object of the given type to the
    /// type of this converter, using the specified context.</summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format
    /// context.</param>
    /// <param name="sourceType">A <see cref="Type" /> that represents the type you want to
    /// convert from.</param>
    /// <returns><see langword="true" /> if this converter can perform the conversion;
    /// otherwise, <see langword="false" />.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string);

    /// <summary>Returns whether this converter can convert the object to the specified type,
    /// using the specified context.</summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format
    /// context.</param>
    /// <param name="destinationType">A <see cref="Type" /> that represents the type you want to
    /// convert to.</param>
    /// <returns><see langword="true" /> if this converter can perform the conversion;
    /// otherwise, <see langword="false" />.</returns>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string);

    /// <summary>Converts the given object to the type of this converter, using the specified
    /// context and culture information.</summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format
    /// context.</param>
    /// <param name="culture">The <see cref="CultureInfo" /> to use as the current
    /// culture.</param>
    /// <param name="value">The <see cref="object" /> to convert.</param>
    /// <returns>An <see cref="object" /> that represents the converted value.</returns>
    /// <exception cref="NotSupportedException">The conversion cannot be performed.</exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string)
        {
            return ConvertFromString(value as string);
        }
        else
        {
            throw new NotSupportedException("Type not supported.");
        }
    }

    /// <summary>Converts the given value object to the specified type, using the specified context and culture information.</summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
    /// <param name="culture">A <see cref="CultureInfo" />. If <see langword="null" /> is passed, the current culture is assumed.</param>
    /// <param name="value">The <see cref="object" /> to convert.</param>
    /// <param name="destinationType">The <see cref="Type" /> to convert the <paramref name="value" /> parameter to.</param>
    /// <returns>An <see cref="object" /> that represents the converted value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="destinationType" /> parameter is <see langword="null" />.</exception>
    /// <exception cref="NotSupportedException">The conversion cannot be performed.</exception>
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType is null)
        {
            throw new ArgumentNullException(nameof(destinationType), "Cannot be null.");
        }
        if (value is not Isotope)
        {
            throw new NotSupportedException("Type of value not supported.");
        }
        if (destinationType == typeof(string))
        {
            return ConvertToString((Isotope)value);
        }
        else
        {
            throw new NotSupportedException("Destination type not supported.");
        }
    }
}
