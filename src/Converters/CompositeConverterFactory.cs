using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tavenem.Mathematics;

namespace Tavenem.Chemistry;

/// <summary>
/// Converts a <see cref="Composite{TScalar}"/> to or from JSON.
/// </summary>
public class CompositeConverterFactory : JsonConverterFactory
{
    /// <summary>Determines whether the specified type can be converted.</summary>
    /// <param name="typeToConvert">The type to compare against.</param>
    /// <returns>
    /// <see langword="true" /> if the type can be converted; otherwise, <see langword="false" />.
    /// </returns>
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        return typeToConvert.GetGenericTypeDefinition().IsAssignableFrom(typeof(Composite<>));
    }

    /// <summary>
    /// Creates a converter for a specified type.
    /// </summary>
    /// <param name="typeToConvert">The type handled by the converter.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns>
    /// A converter for which T is compatible with <paramref name="typeToConvert" />.
    /// </returns>
    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var type = typeToConvert.GetGenericArguments()[0];

        return (JsonConverter)Activator.CreateInstance(
            typeof(CompositeConverter<>).MakeGenericType(
                new Type[] { type }),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: null,
            culture: null)!;
    }

    private class CompositeConverter<TScalar> : JsonConverter<Composite<TScalar>>
         where TScalar : IFloatingPointIeee754<TScalar>
    {
        private const string HasMassPropertyName = "HasMass";

        /// <summary>Reads and converts the JSON to a <see cref="Composite{TScalar}"/>.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override Composite<TScalar>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                nameof(Composite<TScalar>.Components),
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

            var components = new List<IMaterial<TScalar>>();
            var materialConverter = (JsonConverter<IMaterial<TScalar>>)options.GetConverter(typeof(IMaterial<TScalar>));

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }

                var component = materialConverter.Read(ref reader, typeof(IMaterial<TScalar>), options);
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
                nameof(Composite<TScalar>.Density),
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
                HasMassPropertyName,
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
            var hasMass = reader.GetBoolean();

            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            prop = reader.GetString();
            if (!string.Equals(
                prop,
                nameof(Composite<TScalar>.Mass),
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
            TScalar? mass;
            if (reader.TokenType == JsonTokenType.Null)
            {
                mass = default;
            }
            else
            {
                mass = JsonSerializer.Deserialize<TScalar>(ref reader, options);
            }

            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName
                || !reader.ValueTextEquals(nameof(Composite<TScalar>.Shape))
                || !reader.Read()
                || reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            var shape = JsonSerializer.Deserialize<IShape<TScalar>>(ref reader, options);
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
                nameof(Composite<TScalar>.Temperature),
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

            return hasMass && mass is not null
                ? new Composite<TScalar>(
                    components,
                    shape,
                    mass,
                    density,
                    temperature)
                : new Composite<TScalar>(
                    components,
                    shape,
                    density,
                    temperature);
        }

        /// <summary>Writes a <see cref="Composite{TScalar}"/> as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, Composite<TScalar> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteStartArray(nameof(Composite<TScalar>.Components));
            foreach (var component in value.Components)
            {
                JsonSerializer.Serialize(writer, component, component.GetType(), options);
            }
            writer.WriteEndArray();

            if (value._density.HasValue)
            {
                writer.WriteNumber(
                    options.PropertyNamingPolicy is null
                        ? nameof(Composite<TScalar>.Density)
                        : options.PropertyNamingPolicy.ConvertName(nameof(Composite<TScalar>.Density)),
                    value._density.Value);
            }
            else
            {
                writer.WriteNull(nameof(Composite<TScalar>.Density));
            }

            writer.WritePropertyName(options.PropertyNamingPolicy is null
                ? HasMassPropertyName
                : options.PropertyNamingPolicy.ConvertName(HasMassPropertyName));
            JsonSerializer.Serialize(writer, value._hasMass, options);

            if (value._hasMass && value._mass is not null)
            {
                writer.WritePropertyName(options.PropertyNamingPolicy is null
                    ? nameof(Composite<TScalar>.Mass)
                    : options.PropertyNamingPolicy.ConvertName(nameof(Composite<TScalar>.Mass)));
                JsonSerializer.Serialize(writer, value._mass, options);
            }
            else
            {
                writer.WriteNull(options.PropertyNamingPolicy is null
                    ? nameof(Composite<TScalar>.Mass)
                    : options.PropertyNamingPolicy.ConvertName(nameof(Composite<TScalar>.Mass)));
            }

            writer.WritePropertyName(options.PropertyNamingPolicy is null
                ? nameof(Composite<TScalar>.Shape)
                : options.PropertyNamingPolicy.ConvertName(nameof(Composite<TScalar>.Shape)));
            JsonSerializer.Serialize(writer, value.Shape, value.Shape.GetType(), options);

            if (value._temperature.HasValue)
            {
                writer.WriteNumber(
                    options.PropertyNamingPolicy is null
                        ? nameof(Composite<TScalar>.Temperature)
                        : options.PropertyNamingPolicy.ConvertName(nameof(Composite<TScalar>.Temperature)),
                    value._temperature.Value);
            }
            else
            {
                writer.WriteNull(options.PropertyNamingPolicy is null
                    ? nameof(Composite<TScalar>.Temperature)
                    : options.PropertyNamingPolicy.ConvertName(nameof(Composite<TScalar>.Temperature)));
            }

            writer.WriteEndObject();
        }
    }
}
