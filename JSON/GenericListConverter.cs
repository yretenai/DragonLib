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
        public static bool TryReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options, out T value)
        {
            value = default!;
            reader.Read();
            if (reader.TokenType != JsonTokenType.StartArray) return false;

            reader.Read();
            var typeName = reader.GetString();
            if (typeName == null) return false;

            reader.Read();
            var type = Type.GetType(typeName);
            if (type == null) return false;

            value = (reader.TokenType == JsonTokenType.Null ? default : (T) JsonSerializer.Deserialize(ref reader, type, options))!;
            reader.Read();
            return reader.TokenType == JsonTokenType.EndArray;
        }

        public static void WriteValue(Utf8JsonWriter writer, object? value, Type? valueType, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            
            Console.WriteLine(valueType?.AssemblyQualifiedName);
            writer.WriteStringValue(valueType?.AssemblyQualifiedName);

            if (value == null)
                writer.WriteNullValue();
            else
                JsonSerializer.Serialize(writer, value, valueType ?? typeof(T), options);

            writer.WriteEndArray();
        }

        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();

            var list = new List<T>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray) return list;

                if (!TryReadValue(ref reader, options, out var value)) throw new JsonException();

                list.Add(value);
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<T> list, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var value in list)
            {
                WriteValue(writer, value, value?.GetType(), options);
            }

            writer.WriteEndArray();
        }
    }
}
