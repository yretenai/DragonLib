using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonLib.JSON {
    public class GenericTypelessDictionaryConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>> where TKey : notnull {
        public GenericTypelessDictionaryConverter(bool overrideIsStringKey, IEnumerable<Type> additonalTypes) {
            if (!IsStringKey && additonalTypes.Any(x => x.IsAssignableFrom(typeof(TKey))) || overrideIsStringKey) IsStringKey = true;
        }

        public bool IsStringKey { get; } = typeof(TKey) == typeof(string);

        public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> dict, JsonSerializerOptions options) {
            if (IsStringKey)
                writer.WriteStartObject();
            else
                writer.WriteStartArray();

            foreach (var (key, value) in dict) {
                if (IsStringKey) {
                    writer.WritePropertyName(key.ToString() ?? "");
                } else {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Key");
                    JsonSerializer.Serialize(writer, key, key.GetType(), options);
                    writer.WritePropertyName("Value");
                }

                JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(TValue), options);

                if (!IsStringKey) writer.WriteEndObject();
            }

            if (IsStringKey)
                writer.WriteEndObject();
            else
                writer.WriteEndArray();
        }
    }
}
