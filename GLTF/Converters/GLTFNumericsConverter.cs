using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DragonLib.Numerics;

namespace DragonLib.GLTF.Converters
{
    public class GLTFNumericsConverter : JsonConverterFactory
    {
        private static readonly Dictionary<Type, JsonConverter> Converters = new Dictionary<Type, JsonConverter>
        {
            { typeof(Matrix3x3?), new Matrix3x3Converter() },
            { typeof(Matrix4x3?), new Matrix4x3Converter() },
            { typeof(Matrix4x4?), new Matrix4x4Converter() },
            { typeof(Quaternion?), new QuaternionConverter() },
            { typeof(Vector2?), new Vector2Converter() },
            { typeof(Vector3?), new Vector3Converter() },
            { typeof(Vector4?), new Vector4Converter() },
            { typeof(Half?), new HalfConverter() }
        };

        public override bool CanConvert(Type typeToConvert) => Converters.ContainsKey(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => Converters[typeToConvert];
    }
}
