using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DragonLib.Numerics;

namespace DragonLib.GLTF.Converters
{
    public class Matrix4x4Converter : JsonConverter<Matrix4x4?>
    {
        public override Matrix4x4? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            var floats = new float[4 * 4];
            for (var i = 0; i < floats.Length; ++i)
                if (reader.TryGetSingle(out var value))
                    floats[i] = value;
            return new Matrix4x4(floats);
        }

        public override void Write(Utf8JsonWriter writer, Matrix4x4? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            var values = value.Value.ToArray();
            foreach (var floatValue in values) writer.WriteNumberValue(floatValue);
            writer.WriteEndArray();
        }
    }
}
