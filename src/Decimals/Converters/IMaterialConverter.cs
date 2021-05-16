using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tavenem.Chemistry.Decimals
{
    /// <summary>
    /// Converts an <see cref="IMaterial"/> to or from JSON.
    /// </summary>
    public class IMaterialConverter : JsonConverter<IMaterial>
    {
        /// <summary>Determines whether the specified type can be converted.</summary>
        /// <param name="typeToConvert">The type to compare against.</param>
        /// <returns>
        /// <see langword="true" /> if the type can be converted; otherwise, <see langword="false"
        /// />.
        /// </returns>
        public override bool CanConvert(Type typeToConvert) => typeof(IMaterial).IsAssignableFrom(typeToConvert);

        /// <summary>Reads and converts the JSON to an <see cref="IMaterial"/>.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override IMaterial? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var readerCopy = reader;
            readerCopy.Read();
            if (readerCopy.TokenType == JsonTokenType.PropertyName)
            {
                var prop = readerCopy.GetString();
                if (string.Equals(
                    prop,
                    nameof(Composite.Components),
                    options.PropertyNameCaseInsensitive
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal))
                {
                    return JsonSerializer.Deserialize<Composite>(ref reader, options);
                }
            }
            return JsonSerializer.Deserialize<Material>(ref reader, options);
        }

        /// <summary>Writes an <see cref="IMaterial"/> as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, IMaterial value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
