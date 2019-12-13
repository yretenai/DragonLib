using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DragonLib.Numerics;

namespace DragonLib.GLTF.Converters
{
    public class QuaternionConverter : JsonConverter<Quaternion?>
    {
        public override Quaternion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            var floats = new float[4];
            for (var i = 0; i < floats.Length; ++i)
                if (reader.TryGetSingle(out var value))
                    floats[i] = value;
            return new Quaternion(floats);
        }

        public override void Write(Utf8JsonWriter writer, Quaternion? value, JsonSerializerOptions options)
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
