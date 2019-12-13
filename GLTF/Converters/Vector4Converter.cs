using System;
using DragonLib.Numerics;
using Newtonsoft.Json;

namespace DragonLib.GLTF.Converters
{
    public class Vector4Converter : JsonConverter<Vector4?>
    { 
        public override void WriteJson(JsonWriter writer, Vector4? value, JsonSerializer serializer)
        {
            if (!value.HasValue)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();
            var values = value.Value.ToArray();
            foreach (var floatValue in values)
            {
                writer.WriteValue(floatValue);
            }
            writer.WriteEndArray();
        }

        public override Vector4? ReadJson(JsonReader reader, Type objectType, Vector4? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            var floats = new float[3 * 3];
            for (var i = 0; i < floats.Length; ++i)
            {
                floats[i] = (float)(reader.ReadAsDouble() ?? 0d);
            }
            return new Vector4(floats);
        }
    }
}
