using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tavenem.Chemistry;

/// <summary>
/// Converts an <see cref="HomogeneousReference"/> to or from JSON.
/// </summary>
public class HomogeneousReferenceConverter : JsonConverter<HomogeneousReference>
{
    /// <summary>Reads and converts the JSON to an <see cref="ISubstanceReference"/>.</summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted value.</returns>
    public override HomogeneousReference? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var str = reader.GetString();
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }

        if (str.StartsWith("HR:"))
        {
            return new HomogeneousReference(str[3..]);
        }
        return null;
    }

    /// <inheritdoc />
    public override HomogeneousReference ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Read(ref reader, typeToConvert, options) ?? HomogeneousReference.Empty;

    /// <summary>Writes an <see cref="ISubstance"/> as JSON.</summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, HomogeneousReference value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());

    /// <inheritdoc />
    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] HomogeneousReference value, JsonSerializerOptions options)
        => writer.WritePropertyName(value.ToString());
}
