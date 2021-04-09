using System;
using System.ComponentModel;
using System.Globalization;

namespace Tavenem.Chemistry
{
    /// <summary>
    /// Type converter for <see cref="HomogeneousReference"/>. Supports string conversion.
    /// </summary>
    public class HomogeneousReferenceTypeConverter : TypeConverter
    {
        /// <summary>
        /// A static instance of <see cref="HomogeneousReferenceTypeConverter"/>.
        /// </summary>
        public static readonly HomogeneousReferenceTypeConverter Instance = new();

        /// <summary>Converts the given string to an <see cref="HomogeneousReference"/>.</summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>An <see cref="HomogeneousReference" /> that represents the converted
        /// value.</returns>
        public new static HomogeneousReference ConvertFromString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new HomogeneousReference((string?)null);
            }
            else if (value.StartsWith("HR:"))
            {
                return new HomogeneousReference(value[3..]);
            }
            else
            {
                return new HomogeneousReference((string?)null);
            }
        }

        /// <summary>
        /// Converts the given <see cref="HomogeneousReference"/> to a string.
        /// </summary>
        /// <param name="value">The <see cref="HomogeneousReference"/> to convert.</param>
        /// <returns>A string that represents the converted value.</returns>
        public static string ConvertToString(HomogeneousReference value) => $"{value.ReferenceCode}:{value.Id}";

        /// <summary>
        /// Converts the given <see cref="HomogeneousReference"/> to a string.
        /// </summary>
        /// <param name="value">The <see cref="HomogeneousReference"/> to convert.</param>
        /// <returns>A string that represents the converted value.</returns>
        public new static string ConvertToString(object value)
        {
            if (value is null)
            {
                return string.Empty;
            }
            if (value is not HomogeneousReference reference)
            {
                throw new NotSupportedException("Type of value not supported.");
            }
            return ConvertToString(reference);
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

        /// <summary>Converts the given value object to the specified type, using the specified
        /// context and culture information.</summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format
        /// context.</param>
        /// <param name="culture">A <see cref="CultureInfo" />. If <see langword="null" /> is
        /// passed, the current culture is assumed.</param>
        /// <param name="value">The <see cref="object" /> to convert.</param>
        /// <param name="destinationType">The <see cref="Type" /> to convert the <paramref
        /// name="value" /> parameter to.</param>
        /// <returns>An <see cref="object" /> that represents the converted value.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="destinationType" />
        /// parameter is <see langword="null" />.</exception>
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
            if (value is not HomogeneousReference reference)
            {
                throw new NotSupportedException("Type of value not supported.");
            }
            if (destinationType == typeof(string))
            {
                return ConvertToString(reference);
            }
            else
            {
                throw new NotSupportedException("Destination type not supported.");
            }
        }
    }
}
