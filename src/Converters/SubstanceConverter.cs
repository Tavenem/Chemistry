using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Tavenem.Chemistry
{
    /// <summary>
    /// Type converter for <see cref="ISubstance"/>. Supports string conversion.
    /// </summary>
    public class SubstanceConverter : TypeConverter
    {
        /// <summary>
        /// A static instance of <see cref="SubstanceConverter"/>.
        /// </summary>
        public static readonly SubstanceConverter Instance = new();

        /// <summary>Converts the given string to an <see cref="ISubstance"/>.</summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>An <see cref="ISubstance" /> that represents the converted value.</returns>
        public new static ISubstance ConvertFromString(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Chemical.None;
            }
            if (Substances.TryGetSubstance(value, out var substance))
            {
                return substance;
            }
            else
            {
                throw new NotSupportedException("Substance reference not found in registry");
            }
        }

        /// <summary>
        /// Converts the given <see cref="ISubstance"/> to a string.
        /// </summary>
        /// <param name="value">The <see cref="ISubstance"/> to convert.</param>
        /// <returns>A string that represents the converted value.</returns>
        public static string ConvertToString(ISubstance? value) => value?.Id ?? string.Empty;

        /// <summary>Attempts to convert the given string to an <see cref="ISubstance"/>.</summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">If successful, will be set to the <see cref="ISubstance"/> that
        /// represents the converted value.</param>
        /// <returns><see langword="true"/> if the conversion succeeeds; otherwise <see
        /// langword="false"/>.</returns>
        public static bool TryConvertFromString(string? value, [NotNullWhen(true)] out ISubstance? result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = null;
                return false;
            }
            return Substances.TryGetSubstance(value, out result);
        }

        /// <summary>Returns whether this converter can convert an object of the given type to the
        /// type of this converter, using the specified context.</summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format
        /// context.</param>
        /// <param name="sourceType">A <see cref="Type" /> that represents the type you want to
        /// convert from.</param>
        /// <returns><see langword="true" /> if this converter can perform the conversion;
        /// otherwise, <see langword="false" />.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string);

        /// <summary>Returns whether this converter can convert the object to the specified type,
        /// using the specified context.</summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format
        /// context.</param>
        /// <param name="destinationType">A <see cref="Type" /> that represents the type you want to
        /// convert to.</param>
        /// <returns><see langword="true" /> if this converter can perform the conversion;
        /// otherwise, <see langword="false" />.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
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
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
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
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType is null)
            {
                throw new ArgumentNullException(nameof(destinationType), "Cannot be null.");
            }
            if (value is null)
            {
                return string.Empty;
            }
            if (value is not ISubstance)
            {
                throw new NotSupportedException("Type of value not supported.");
            }
            if (destinationType == typeof(string))
            {
                return ConvertToString(value as ISubstance);
            }
            else
            {
                throw new NotSupportedException("Destination type not supported.");
            }
        }
    }
}
