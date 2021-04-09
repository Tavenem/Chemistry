using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tavenem.DataStorage;

namespace Tavenem.Chemistry
{
    /// <summary>
    /// Converts an <see cref="ISubstance"/> to or from JSON.
    /// </summary>
    public class ISubstanceConverter : JsonConverter<ISubstance>
    {
        /// <summary>Determines whether the specified type can be converted.</summary>
        /// <param name="typeToConvert">The type to compare against.</param>
        /// <returns>
        /// <see langword="true" /> if the type can be converted; otherwise, <see langword="false"
        /// />.
        /// </returns>
        public override bool CanConvert(Type typeToConvert) => typeof(ISubstance).IsAssignableFrom(typeToConvert);

        /// <summary>Reads and converts the JSON to an <see cref="ISubstance"/>.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override ISubstance? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
            while (readerCopy.Read())
            {
                if (readerCopy.TokenType == JsonTokenType.PropertyName)
                {
                    var prop = readerCopy.GetString();
                    if (string.Equals(
                        prop,
                        nameof(IIdItem.IdItemTypeName),
                        options.PropertyNameCaseInsensitive
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.Ordinal))
                    {
                        if (!readerCopy.Read()
                            || readerCopy.TokenType != JsonTokenType.String)
                        {
                            throw new JsonException("Type discriminator missing or invalid");
                        }
                        var classTypeString = readerCopy.GetString();
                        if (string.IsNullOrEmpty(classTypeString))
                        {
                            throw new JsonException("Type discriminator missing or invalid");
                        }
                        return classTypeString switch
                        {
                            Chemical.ChemicalIdItemTypeName
                                => JsonSerializer.Deserialize(ref reader, typeof(Chemical), options) as ISubstance,
                            HomogeneousSubstance.HomogeneousSubstanceIdItemTypeName
                                => JsonSerializer.Deserialize(ref reader, typeof(HomogeneousSubstance), options) as ISubstance,
                            Mixture.MixtureIdItemTypeName
                                => JsonSerializer.Deserialize(ref reader, typeof(Mixture), options) as ISubstance,
                            Solution.SolutionIdItemTypeName
                                => JsonSerializer.Deserialize(ref reader, typeof(Solution), options) as ISubstance,
                            _ => throw new JsonException("Type discriminator invalid"),
                        };
                    }
                }
            }
            throw new JsonException("Type discriminator missing");
        }

        /// <summary>Writes an <see cref="ISubstance"/> as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, ISubstance value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
