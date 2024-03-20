using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tavenem.Chemistry;

/// <summary>
/// Converts an <see cref="IMaterial{TScalar}"/> to or from JSON.
/// </summary>
public class MaterialConverterFactory : JsonConverterFactory
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

        return typeToConvert.GetGenericTypeDefinition().IsAssignableFrom(typeof(IMaterial<>));
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
            typeof(MaterialConverter<>).MakeGenericType(
                [type]),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: null,
            culture: null)!;
    }

    private class MaterialConverter<TScalar> : JsonConverter<IMaterial<TScalar>>
         where TScalar : IFloatingPointIeee754<TScalar>
    {
        /// <summary>Reads and converts the JSON to an <see cref="IMaterial{TScalar}"/>.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override IMaterial<TScalar>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                    nameof(Composite<TScalar>.Components),
                    options.PropertyNameCaseInsensitive
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal))
                {
                    return JsonSerializer.Deserialize<Composite<TScalar>>(ref reader, options);
                }
            }
            return JsonSerializer.Deserialize<Material<TScalar>>(ref reader, options);
        }

        /// <summary>Writes an <see cref="IMaterial{TScalar}"/> as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, IMaterial<TScalar> value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
