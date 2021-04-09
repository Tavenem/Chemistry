using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tavenem.Chemistry
{
    /// <summary>
    /// Converts an <see cref="IReadOnlyDictionary{TKey, TValue}"/> of <see
    /// cref="HomogeneousReference"/> and <see cref="decimal"/> to or from JSON.
    /// </summary>
    public class SubstanceConstituentsConverter : JsonConverter<IReadOnlyDictionary<HomogeneousReference, decimal>>
    {
        /// <summary>Determines whether the specified type can be converted.</summary>
        /// <param name="typeToConvert">The type to compare against.</param>
        /// <returns>
        /// <see langword="true" /> if the type can be converted; otherwise, <see langword="false"
        /// />.
        /// </returns>
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(IReadOnlyDictionary<,>))
            {
                return false;
            }

            return typeToConvert.GetGenericArguments()[0] == typeof(HomogeneousReference)
                && typeToConvert.GetGenericArguments()[1] == typeof(decimal);
        }

        /// <summary>Reads and converts the JSON to an <see cref="IReadOnlyDictionary{TKey,
        /// TValue}"/> of <see cref="HomogeneousReference"/> and <see cref="decimal"/>.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override IReadOnlyDictionary<HomogeneousReference, decimal>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = new Dictionary<HomogeneousReference, decimal>();
            var decimalConverter = (JsonConverter<decimal>)options.GetConverter(typeof(decimal));

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new ReadOnlyDictionary<HomogeneousReference, decimal>(dictionary);
                }

                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();

                if (string.IsNullOrWhiteSpace(propertyName)
                    || !propertyName.StartsWith(
                        "HR:",
                        options.PropertyNameCaseInsensitive
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.Ordinal))
                {
                    throw new JsonException();
                }

                var key = new HomogeneousReference(propertyName[3..]);

                // Get the value.
                if (!reader.Read())
                {
                    throw new JsonException();
                }
                var v = decimalConverter.Read(ref reader, typeof(decimal), options);

                // Add to dictionary.
                dictionary.Add(key, v);
            }

            throw new JsonException();
        }

        /// <summary>Writes an <see cref="IReadOnlyDictionary{TKey, TValue}"/> of <see
        /// cref="HomogeneousReference"/> and <see cref="decimal"/> as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<HomogeneousReference, decimal> value, JsonSerializerOptions options)
        {
            var decimalConverter = (JsonConverter<decimal>)options.GetConverter(typeof(decimal));

            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                var keyString = kvp.Key.ToString();
                if (string.IsNullOrWhiteSpace(keyString))
                {
                    throw new JsonException();
                }
                writer.WritePropertyName(
                    options.PropertyNamingPolicy is null
                        ? keyString
                        : options.PropertyNamingPolicy.ConvertName(keyString));

                decimalConverter.Write(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}
