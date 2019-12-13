using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    [JsonConverter(typeof(JsonStringEnumConverter))]
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
