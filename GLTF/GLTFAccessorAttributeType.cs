using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming

namespace DragonLib.GLTF
{
    [PublicAPI]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GLTFAccessorAttributeType
    {
        SCALAR,
        VEC2,
        VEC3,
        VEC4,
        MAT2,
        MAT3,
        MAT4
    }
}
