using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.JSON
{
    [PublicAPI]
    public class GenericDictionaryConverter<T> : JsonConverter<Dictionary<string, T>>
    {
        public override Dictionary<string, T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

            var dict = new Dictionary<string, T>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) return dict;
                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();

                var key = reader.GetString();
                reader.Read();
                if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();

                reader.Read();
                var typeinfo = reader.GetString() ?? throw new JsonException();
                reader.Read();
                var type = Type.GetType(typeinfo) ?? throw new TypeUnloadedException();
                var value = JsonSerializer.Deserialize(ref reader, type, options);
                dict[key!] = (T) value!;
                reader.Read();
                if (reader.TokenType != JsonTokenType.EndArray) throw new JsonException();
            }

            return dict;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, T> dict, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var (key, value) in dict)
            {
                writer.WritePropertyName(key);
                writer.WriteStartArray();
                writer.WriteStringValue(value?.GetType().AssemblyQualifiedName);
                JsonSerializer.Serialize(writer, value, options);
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
