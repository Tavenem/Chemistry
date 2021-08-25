using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tavenem.Chemistry;

/// <summary>
/// Converts an <see cref="ISubstanceReference"/> to or from JSON.
/// </summary>
public class ISubstanceReferenceConverter : JsonConverter<ISubstanceReference>
{
    /// <summary>Determines whether the specified type can be converted.</summary>
    /// <param name="typeToConvert">The type to compare against.</param>
    /// <returns>
    /// <see langword="true" /> if the type can be converted; otherwise, <see langword="false"
    /// />.
    /// </returns>
    public override bool CanConvert(Type typeToConvert) => typeof(ISubstanceReference).IsAssignableFrom(typeToConvert);

    /// <summary>Reads and converts the JSON to an <see cref="ISubstanceReference"/>.</summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted value.</returns>
    public override ISubstanceReference? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        if (str.StartsWith("SR:"))
        {
            return new SubstanceReference(str[3..]);
        }
        if (str.StartsWith("HR:"))
        {
            return new HomogeneousReference(str[3..]);
        }
        return null;
    }

    /// <summary>Writes an <see cref="ISubstance"/> as JSON.</summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, ISubstanceReference value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
