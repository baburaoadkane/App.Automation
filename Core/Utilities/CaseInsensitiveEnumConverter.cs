using System.Text.Json;
using System.Text.Json.Serialization;

namespace App.Automation.Core.Utilities
{
    /// <summary>
    /// A custom JSON converter for enums that performs case-insensitive deserialization.
    /// Allows JSON enum values in any case (camelCase, PascalCase, UPPERCASE, snake_case) to match enum members.
    /// </summary>
    public class CaseInsensitiveEnumConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(CaseInsensitiveEnumConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter?)Activator.CreateInstance(converterType);
        }
    }

    /// <summary>
    /// Generic implementation of case-insensitive enum converter.
    /// </summary>
    public class CaseInsensitiveEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? enumValue = reader.GetString();
                if (enumValue != null)
                {
                    // Try to parse the enum value case-insensitively
                    if (Enum.TryParse<T>(enumValue, ignoreCase: true, out var result))
                    {
                        return result;
                    }

                    // If parsing fails, throw a detailed exception
                    var validValues = string.Join(", ", Enum.GetNames(typeof(T)));
                    throw new JsonException(
                        $"Unable to convert \"{enumValue}\" to enum type {typeof(T).Name}. " +
                        $"Valid values are: {validValues}");
                }
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when parsing enum {typeof(T).Name}.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
