using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonLib.Text.Json;

public class JsonNullableNumberEnumConverter : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum || typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Nullable<>) && typeToConvert.GenericTypeArguments[0].IsEnum;
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter?) Activator.CreateInstance(typeof(JsonNullableNumberEnumConverter<>).MakeGenericType(typeToConvert));
}

public class JsonNullableNumberEnumConverter<T> : JsonConverter<T?> where T : struct, Enum {
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
                   JsonTokenType.Number => (T) Enum.ToObject(typeof(T), reader.GetInt64()),
                   JsonTokenType.String => (T) Enum.Parse(typeof(T), reader.GetString() ?? string.Empty, true),
                   JsonTokenType.Null   => null,
                   _                    => throw new JsonException("Unexpected token type"),
               };
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options) {
        if (value == null) {
            writer.WriteNullValue();
            return;
        }

        writer.WriteNumberValue(Convert.ToInt64(value));
    }

    public override bool CanConvert(Type typeToConvert) {
        return typeToConvert.IsEnum;
    }

    public override bool HandleNull => true;
}
