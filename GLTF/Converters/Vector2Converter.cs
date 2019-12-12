using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DragonLib.Numerics;

namespace DragonLib.GLTF.Converters
{
    public class Vector2Converter : JsonConverter<Vector2?>
    {
        public override Vector2? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            var floats = new float[2];
            for (var i = 0; i < floats.Length; ++i)
            {
                if (reader.TryGetSingle(out var value)) floats[i] = value;
            }
            return new Vector2(floats);
        }

        public override void Write(Utf8JsonWriter writer, Vector2? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }
            
            writer.WriteStartArray();
            var values = value.Value.ToArray();
            foreach (var floatValue in values)
            {
                writer.WriteNumberValue(floatValue);
            }
            writer.WriteEndArray();
        }
    }
}
