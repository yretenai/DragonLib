using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DragonLib.Numerics;

namespace DragonLib.GLTF.Converters
{
    public class HalfConverter : JsonConverter<Half?>
    {
        public override Half? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            return reader.TryGetSingle(out var value) ? new Half(value) : (Half?) null;
        }

        public override void Write(Utf8JsonWriter writer, Half? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }
            writer.WriteNumberValue(value.Value);
        }
    }
}
