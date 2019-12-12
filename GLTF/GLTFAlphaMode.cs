using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI, JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GLTFAlphaMode
    {
        OPAQUE,
        MASK,
        BLEND
    }
}
