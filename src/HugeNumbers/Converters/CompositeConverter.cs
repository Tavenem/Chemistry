using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tavenem.HugeNumbers;
using Tavenem.Mathematics.HugeNumbers;

namespace Tavenem.Chemistry.HugeNumbers
{
    /// <summary>
    /// Converts a <see cref="Composite"/> to or from JSON.
    /// </summary>
    public class CompositeConverter : JsonConverter<Composite>
    {
        /// <summary>Determines whether the specified type can be converted.</summary>
        /// <param name="typeToConvert">The type to compare against.</param>
        /// <returns>
        /// <see langword="true" /> if the type can be converted; otherwise, <see langword="false"
        /// />.
        /// </returns>
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Composite);

        /// <summary>Reads and converts the JSON to a <see cref="Composite"/>.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override Composite? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            var prop = reader.GetString();
            if (!string.Equals(
                prop,
                nameof(Composite.Components),
                options.PropertyNameCaseInsensitive
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal))
            {
                throw new JsonException();
            }

            if (!reader.Read()
                || reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            var components = new List<IMaterial>();
            var materialConverter = (JsonConverter<IMaterial>)options.GetConverter(typeof(IMaterial));

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }

                var component = materialConverter.Read(ref reader, typeof(IMaterial), options);
                if (component is not null)
                {
                    components.Add(component);
                }
            }

            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            prop = reader.GetString();
            if (!string.Equals(
                prop,
                nameof(Composite.Density),
                options.PropertyNameCaseInsensitive
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal))
            {
                throw new JsonException();
            }
            if (!reader.Read())
            {
                throw new JsonException();
            }
            double? density;
            if (reader.TokenType == JsonTokenType.Null)
            {
                density = null;
            }
            else if (reader.TryGetDouble(out var densityValue))
            {
                density = densityValue;
            }
            else
            {
                throw new JsonException();
            }

            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            prop = reader.GetString();
            if (!string.Equals(
                prop,
                nameof(Composite.Mass),
                options.PropertyNameCaseInsensitive
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal))
            {
                throw new JsonException();
            }
            if (!reader.Read())
            {
                throw new JsonException();
            }
            HugeNumber? mass;
            if (reader.TokenType == JsonTokenType.Null)
            {
                mass = null;
            }
            else if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }
            else
            {
                mass = JsonSerializer.Deserialize<HugeNumber>(ref reader, options);
            }

            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName
                || !reader.ValueTextEquals(nameof(Composite.Shape))
                || !reader.Read()
                || reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            var shape = JsonSerializer.Deserialize<IShape>(ref reader, options);
            if (shape is null)
            {
                throw new JsonException();
            }
            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            prop = reader.GetString();
            if (!string.Equals(
                prop,
                nameof(Composite.Temperature),
                options.PropertyNameCaseInsensitive
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal))
            {
                throw new JsonException();
            }
            if (!reader.Read())
            {
                throw new JsonException();
            }
            double? temperature;
            if (reader.TokenType == JsonTokenType.Null)
            {
                temperature = null;
            }
            else if (reader.TryGetDouble(out var temperatureValue))
            {
                temperature = temperatureValue;
            }
            else
            {
                throw new JsonException();
            }

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                reader.Read();
            }

            return new Composite(
                components,
                shape,
                density,
                mass,
                temperature);
        }

        /// <summary>Writes a <see cref="Composite"/> as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, Composite value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteStartArray(nameof(Composite.Components));
            foreach (var component in value.Components)
            {
                JsonSerializer.Serialize(writer, component, component.GetType(), options);
            }
            writer.WriteEndArray();

            if (value._density.HasValue)
            {
                writer.WriteNumber(
                    options.PropertyNamingPolicy is null
                        ? nameof(Composite.Density)
                        : options.PropertyNamingPolicy.ConvertName(nameof(Composite.Density)),
                    value._density.Value);
            }
            else
            {
                writer.WriteNull(nameof(Composite.Density));
            }

            if (value._mass.HasValue)
            {
                writer.WritePropertyName(options.PropertyNamingPolicy is null
                    ? nameof(Composite.Mass)
                    : options.PropertyNamingPolicy.ConvertName(nameof(Composite.Mass)));
                JsonSerializer.Serialize(writer, value._mass.Value, options);
            }
            else
            {
                writer.WriteNull(options.PropertyNamingPolicy is null
                    ? nameof(Composite.Mass)
                    : options.PropertyNamingPolicy.ConvertName(nameof(Composite.Mass)));
            }

            writer.WritePropertyName(options.PropertyNamingPolicy is null
                ? nameof(Composite.Shape)
                : options.PropertyNamingPolicy.ConvertName(nameof(Composite.Shape)));
            JsonSerializer.Serialize(writer, value.Shape, value.Shape.GetType(), options);

            if (value._temperature.HasValue)
            {
                writer.WriteNumber(
                    options.PropertyNamingPolicy is null
                        ? nameof(Composite.Temperature)
                        : options.PropertyNamingPolicy.ConvertName(nameof(Composite.Temperature)),
                    value._temperature.Value);
            }
            else
            {
                writer.WriteNull(options.PropertyNamingPolicy is null
                    ? nameof(Composite.Temperature)
                    : options.PropertyNamingPolicy.ConvertName(nameof(Composite.Temperature)));
            }

            writer.WriteEndObject();
        }
    }
}
