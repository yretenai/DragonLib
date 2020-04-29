using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.JSON
{
    [PublicAPI]
    public class GenericListConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();

            var list = new List<T>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray) return list;
                if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();
                reader.Read();
                var typeinfo = reader.GetString() ?? throw new JsonException();
                reader.Read();
                var type = Type.GetType(typeinfo) ?? throw new TypeUnloadedException();
                var value = JsonSerializer.Deserialize(ref reader, type, options);
                list.Add((T) value!);
                reader.Read();
                if (reader.TokenType != JsonTokenType.EndArray) throw new JsonException();
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<T> list, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var value in list)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(value?.GetType().AssemblyQualifiedName);
                JsonSerializer.Serialize(writer, value, value?.GetType(), options);
                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
