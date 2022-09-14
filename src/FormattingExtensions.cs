using System.Globalization;
using System.Numerics;
using System.Text;

namespace Tavenem.Chemistry;

internal static class FormattingExtensions
{
    private static readonly Dictionary<char, byte> _Digits = new()
    {
        { '0', 0 },
        { '1', 1 },
        { '2', 2 },
        { '3', 3 },
        { '4', 4 },
        { '5', 5 },
        { '6', 6 },
        { '7', 7 },
        { '8', 8 },
        { '9', 9 },
    };
    private static readonly Dictionary<char, char> _DigitToSubscript = new()
    {
        { '0', '\u2080' },
        { '1', '\u2081' },
        { '2', '\u2082' },
        { '3', '\u2083' },
        { '4', '\u2084' },
        { '5', '\u2085' },
        { '6', '\u2086' },
        { '7', '\u2087' },
        { '8', '\u2088' },
        { '9', '\u2089' },
        { '+', '\u208A' },
        { '-', '\u208B' },
    };
    private static readonly Dictionary<char, char> _DigitToSuperscript = new()
    {
        { '0', '\u2070' },
        { '1', '\u00b9' },
        { '2', '\u00b2' },
        { '3', '\u00b3' },
        { '4', '\u2074' },
        { '5', '\u2075' },
        { '6', '\u2076' },
        { '7', '\u2077' },
        { '8', '\u2078' },
        { '9', '\u2079' },
        { '+', '\u207A' },
        { '-', '\u207B' },
    };
    private static readonly Dictionary<char, byte> _SubscriptDigits = new()
    {
        { '\u2080', 0 },
        { '\u2081', 1 },
        { '\u2082', 2 },
        { '\u2083', 3 },
        { '\u2084', 4 },
        { '\u2085', 5 },
        { '\u2086', 6 },
        { '\u2087', 7 },
        { '\u2088', 8 },
        { '\u2089', 9 },
    };
    private static readonly Dictionary<char, byte> _SuperscriptDigits = new()
    {
        { '\u2070', 0 },
        { '\u00b9', 1 },
        { '\u00b2', 2 },
        { '\u00b3', 3 },
        { '\u2074', 4 },
        { '\u2075', 5 },
        { '\u2076', 6 },
        { '\u2077', 7 },
        { '\u2078', 8 },
        { '\u2079', 9 },
    };

    /// <summary>
    /// Converts <paramref name="value"/> to a subscript representation.
    /// </summary>
    /// <param name="value">A number to render as a subscript string.</param>
    /// <param name="format">
    /// The format to use. -or- A null reference (Nothing in Visual Basic) to use the
    /// default format defined for the type of the <see cref="IFormattable"/> implementation.
    /// </param>
    /// <param name="formatProvider">
    /// The provider to use to format the value. -or- A null reference (Nothing in Visual
    /// Basic) to obtain the numeric format information from the current locale setting
    /// of the operating system.
    /// </param>
    /// <returns>The subscript representation of <paramref name="value"/>.</returns>
    /// <remarks>
    /// <para>
    /// Due to Unicode limitations, only digits, the '+' character, and the '-' character are converted.
    /// Any other characters produced by the string representation of <paramref name="value"/> will be discarded.
    /// If this would result in an empty string, the original string is returned unaltered.
    /// </para>
    /// <para>
    /// To preserve accuracy, any characters following the <see cref="NumberFormatInfo.NumberDecimalSeparator"/>
    /// (if present) will be discarded, since the <see cref="NumberFormatInfo.NumberDecimalSeparator"/> itself
    /// cananot be represented.
    /// </para>
    /// </remarks>
    public static string ToSubscript<T>(this T value, string? format = null, IFormatProvider? formatProvider = null)
        where T : INumberBase<T>, IFormattable
    {
        var s = value.ToString(format, formatProvider);

        var separator = NumberFormatInfo.GetInstance(formatProvider).NumberDecimalSeparator.AsSpan();
        var sb = new StringBuilder();
        foreach (var c in s)
        {
            if (separator.Equals(new ReadOnlySpan<char>(new[] { c }), StringComparison.InvariantCulture))
            {
                break;
            }
            if (_DigitToSubscript.TryGetValue(c, out var digit))
            {
                sb.Append(digit);
            }
        }
        return sb.Length > 0
            ? sb.ToString()
            : s;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a subscript representation.
    /// </summary>
    /// <param name="value">A number to render as a subscript string.</param>
    /// <param name="formatProvider">
    /// The provider to use to format the value. -or- A null reference (Nothing in Visual
    /// Basic) to obtain the numeric format information from the current locale setting
    /// of the operating system.
    /// </param>
    /// <returns>The subscript representation of <paramref name="value"/>.</returns>
    /// <remarks>
    /// <para>
    /// Due to Unicode limitations, only digits, the '+' character, and the '-' character are converted.
    /// Any other characters produced by the string representation of <paramref name="value"/> will be discarded.
    /// If this would result in an empty string, the original string is returned unaltered.
    /// </para>
    /// <para>
    /// To preserve accuracy, any characters following the <see cref="NumberFormatInfo.NumberDecimalSeparator"/>
    /// (if present) will be discarded, since the <see cref="NumberFormatInfo.NumberDecimalSeparator"/> itself
    /// cananot be represented.
    /// </para>
    /// </remarks>
    public static string ToSubscript<T>(this T value, IFormatProvider formatProvider) where T
        : INumberBase<T>, IFormattable
        => ToSubscript(value, null, formatProvider);

    /// <summary>
    /// Converts <paramref name="value"/> to a superscript representation.
    /// </summary>
    /// <param name="value">A number to render as a superscript string.</param>
    /// <param name="format">
    /// The format to use. -or- A null reference (Nothing in Visual Basic) to use the
    /// default format defined for the type of the <see cref="IFormattable"/> implementation.
    /// </param>
    /// <param name="formatProvider">
    /// The provider to use to format the value. -or- A null reference (Nothing in Visual
    /// Basic) to obtain the numeric format information from the current locale setting
    /// of the operating system.
    /// </param>
    /// <param name="positiveSign">
    /// Causes the superscript string to contain the character '⁺' when non-negative.
    /// </param>
    /// <param name="postfixSign">
    /// Causes the superscript string to end with a sign, overriding the value of <see cref="NumberFormatInfo.NumberNegativePattern"/>.
    /// </param>
    /// <returns>The superscript representation of <paramref name="value"/>.</returns>
    /// <remarks>
    /// <para>
    /// Due to Unicode limitations, only digits, the '+' character, and the '-' character are converted.
    /// Any other characters produced by the string representation of <paramref name="value"/> will be discarded.
    /// If this would result in an empty string, the original string is returned unaltered.
    /// </para>
    /// <para>
    /// To preserve accuracy, any characters following the <see cref="NumberFormatInfo.NumberDecimalSeparator"/>
    /// (if present) will be discarded, since the <see cref="NumberFormatInfo.NumberDecimalSeparator"/> itself
    /// cananot be represented.
    /// </para>
    /// </remarks>
    public static string ToSuperscript<T>(
        this T value,
        string? format = null,
        IFormatProvider? formatProvider = null,
        bool positiveSign = false,
        bool postfixSign = false) where T : INumberBase<T>, IComparable<T>, IFormattable
    {
        var formatInfo = (NumberFormatInfo)NumberFormatInfo.GetInstance(formatProvider).Clone();
        if (postfixSign)
        {
            formatInfo.NumberNegativePattern = formatInfo.NumberNegativePattern == 2
                ? 4
                : 3;
        }

        var s = value.ToString(format, formatInfo);

        var separator = formatInfo.NumberDecimalSeparator.AsSpan();
        var sb = new StringBuilder();
        var showPositive = value.CompareTo(T.Zero) > 0
            && positiveSign;
        if (showPositive
            && formatInfo.NumberNegativePattern < 3)
        {
            sb.Append('\u207A');
        }
        foreach (var c in s)
        {
            if (separator.Equals(new ReadOnlySpan<char>(new[] { c }), StringComparison.InvariantCulture))
            {
                break;
            }
            if (_DigitToSuperscript.TryGetValue(c, out var digit))
            {
                sb.Append(digit);
            }
        }
        if (showPositive
            && formatInfo.NumberNegativePattern >= 3)
        {
            sb.Append('\u207A');
        }
        return sb.Length > 0
            ? sb.ToString()
            : s;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a superscript representation.
    /// </summary>
    /// <param name="value">A number to render as a superscript string.</param>
    /// <param name="formatProvider">
    /// The provider to use to format the value. -or- A null reference (Nothing in Visual
    /// Basic) to obtain the numeric format information from the current locale setting
    /// of the operating system.
    /// </param>
    /// <param name="positiveSign">
    /// Causes the superscript string to contain the character '⁺' when non-negative.
    /// </param>
    /// <param name="postfixSign">
    /// Causes the superscript string to end with a sign, overriding the value of <see cref="NumberFormatInfo.NumberNegativePattern"/>.
    /// </param>
    /// <returns>The superscript representation of <paramref name="value"/>.</returns>
    /// <remarks>
    /// <para>
    /// Due to Unicode limitations, only digits, the '+' character, and the '-' character are converted.
    /// Any other characters produced by the string representation of <paramref name="value"/> will be discarded.
    /// If this would result in an empty string, the original string is returned unaltered.
    /// </para>
    /// <para>
    /// To preserve accuracy, any characters following the <see cref="NumberFormatInfo.NumberDecimalSeparator"/>
    /// (if present) will be discarded, since the <see cref="NumberFormatInfo.NumberDecimalSeparator"/> itself
    /// cananot be represented.
    /// </para>
    /// </remarks>
    public static string ToSuperscript<T>(
        this T value,
        IFormatProvider formatProvider,
        bool positiveSign = false,
        bool postfixSign = false) where T : INumberBase<T>, IComparable<T>, IFormattable
        => ToSuperscript(value, null, formatProvider, positiveSign, postfixSign);

    /// <summary>
    /// Converts <paramref name="value"/> to a superscript representation.
    /// </summary>
    /// <param name="value">A number to render as a superscript string.</param>
    /// <param name="positiveSign">
    /// Causes the superscript string to contain the character '⁺' when non-negative.
    /// </param>
    /// <param name="postfixSign">
    /// Causes the superscript string to end with a sign, overriding the value of <see cref="NumberFormatInfo.NumberNegativePattern"/>.
    /// </param>
    /// <returns>The superscript representation of <paramref name="value"/>.</returns>
    /// <remarks>
    /// <para>
    /// Due to Unicode limitations, only digits, the '+' character, and the '-' character are converted.
    /// Any other characters produced by the string representation of <paramref name="value"/> will be discarded.
    /// If this would result in an empty string, the original string is returned unaltered.
    /// </para>
    /// <para>
    /// To preserve accuracy, any characters following the <see cref="NumberFormatInfo.NumberDecimalSeparator"/>
    /// (if present) will be discarded, since the <see cref="NumberFormatInfo.NumberDecimalSeparator"/> itself
    /// cananot be represented.
    /// </para>
    /// </remarks>
    public static string ToSuperscript<T>(
        this T value,
        bool positiveSign,
        bool postfixSign = false) where T : INumberBase<T>, IComparable<T>, IFormattable
        => ToSuperscript(value, null, null, positiveSign, postfixSign);

    /// <summary>
    /// Attempts to parse the given character as a digit.
    /// </summary>
    /// <param name="c">The character to parse.</param>
    /// <param name="value">
    /// The digit represented by <paramref name="c"/>, or 0 if <paramref name="c"/> could not be
    /// parsed as a digit.
    /// </param>
    /// <param name="superscript">
    /// <see langword="true"/> if the character is the superscript version of the parsed digit.
    /// </param>
    /// <param name="subscript">
    /// <see langword="true"/> if the character is the subscript version of the parsed digit.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the character could be successfully parsed as a digit;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryParseDigit(this char c, out byte value, out bool superscript, out bool subscript)
    {
        superscript = false;
        subscript = false;
        if (_Digits.TryGetValue(c, out value))
        {
            return true;
        }
        if (_SubscriptDigits.TryGetValue(c, out value))
        {
            subscript = true;
            return true;
        }
        if (_SuperscriptDigits.TryGetValue(c, out value))
        {
            superscript = true;
            return true;
        }
        return false;
    }
}
