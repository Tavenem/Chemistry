using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tavenem.Chemistry;

/// <summary>
/// Converts a <see cref="Formula"/> to or from JSON.
/// </summary>
public class FormulaConverter : JsonConverter<Formula>
{
    /// <summary>Reads and converts the JSON to type <see cref="Formula"/>.</summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted value.</returns>
    public override Formula Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Formula.Parse(reader.GetString());

    /// <summary>Writes a specified value as JSON.</summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, Formula value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
