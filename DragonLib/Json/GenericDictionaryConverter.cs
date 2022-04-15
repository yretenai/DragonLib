using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonLib.Json;

public class GenericDictionaryConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>> where TKey : notnull {
    public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) {
            throw new JsonException();
        }

        var dict = new Dictionary<TKey, TValue>();

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndArray) {
                return dict;
            }

            if (reader.TokenType != JsonTokenType.StartObject) {
                throw new JsonException();
            }

            if (!GenericListConverter<TKey>.TryReadValue(ref reader, options, out var key)) {
                throw new JsonException();
            }

            if (!GenericListConverter<TValue>.TryReadValue(ref reader, options, out var value)) {
                throw new JsonException();
            }

            dict[key] = value;
        }

        return dict;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> dict, JsonSerializerOptions options) {
        writer.WriteStartArray();

        foreach (var (key, value) in dict) {
            writer.WriteStartObject();

            writer.WritePropertyName("Key");

            GenericListConverter<TKey>.WriteValue(writer, key, key.GetType(), options);

            writer.WritePropertyName("Value");

            GenericListConverter<TValue>.WriteValue(writer, value, value?.GetType(), options);

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
