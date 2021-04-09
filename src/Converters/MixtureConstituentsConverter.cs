using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tavenem.Chemistry
{
    /// <summary>
    /// Converts an <see cref="IReadOnlyDictionary{TKey, TValue}"/> of <see
    /// cref="ISubstanceReference"/> and <see cref="decimal"/> to or from JSON.
    /// </summary>
    public class MixtureConstituentsConverter : JsonConverter<IReadOnlyDictionary<ISubstanceReference, decimal>>
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

            return typeToConvert.GetGenericArguments()[0] == typeof(ISubstanceReference)
                && typeToConvert.GetGenericArguments()[1] == typeof(decimal);
        }

        /// <summary>Reads and converts the JSON to an <see cref="IReadOnlyDictionary{TKey,
        /// TValue}"/> of <see cref="ISubstanceReference"/> and <see cref="decimal"/>.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override IReadOnlyDictionary<ISubstanceReference, decimal>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = new Dictionary<ISubstanceReference, decimal>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new ReadOnlyDictionary<ISubstanceReference, decimal>(dictionary);
                }

                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException();
                }

                ISubstanceReference key;
                if (propertyName.StartsWith(
                    "SR:",
                    options.PropertyNameCaseInsensitive
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal))
                {
                    key = new SubstanceReference(propertyName[3..]);
                }
                else if (propertyName.StartsWith(
                    "HR:",
                    options.PropertyNameCaseInsensitive
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal))
                {
                    key = new HomogeneousReference(propertyName[3..]);
                }
                else
                {
                    throw new JsonException();
                }

                // Get the value.
                if (!reader.Read())
                {
                    throw new JsonException();
                }
                var v = reader.GetDecimal();

                // Add to dictionary.
                dictionary.Add(key, v);
            }

            throw new JsonException();
        }

        /// <summary>Writes an <see cref="IReadOnlyDictionary{TKey, TValue}"/> of <see
        /// cref="ISubstanceReference"/> and <see cref="decimal"/> as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<ISubstanceReference, decimal> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                var keyString = kvp.Key.ToString();
                if (string.IsNullOrWhiteSpace(keyString))
                {
                    throw new JsonException();
                }
                writer.WriteNumber(
                    options.PropertyNamingPolicy is null
                        ? keyString
                        : options.PropertyNamingPolicy.ConvertName(keyString),
                    kvp.Value);
            }

            writer.WriteEndObject();
        }
    }
}
