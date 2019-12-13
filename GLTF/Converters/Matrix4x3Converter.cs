using System;
using DragonLib.Numerics;
using Newtonsoft.Json;

namespace DragonLib.GLTF.Converters
{
    public class Matrix4x3Converter : JsonConverter<Matrix4x3?>
    {
        public override void WriteJson(JsonWriter writer, Matrix4x3? value, JsonSerializer serializer)
        {
            if (!value.HasValue)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();
            var values = value.Value.ToArray();
            foreach (var floatValue in values) writer.WriteValue(floatValue);
            writer.WriteEndArray();
        }

        public override Matrix4x3? ReadJson(JsonReader reader, Type objectType, Matrix4x3? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var floats = new float[3 * 3];
            for (var i = 0; i < floats.Length; ++i) floats[i] = (float) (reader.ReadAsDouble() ?? 0d);
            return new Matrix4x3(floats);
        }
    }
}
