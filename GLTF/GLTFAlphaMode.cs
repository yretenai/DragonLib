using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming

namespace DragonLib.GLTF
{
    [PublicAPI]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GLTFAlphaMode
    {
        OPAQUE,
        MASK,
        BLEND
    }
}
