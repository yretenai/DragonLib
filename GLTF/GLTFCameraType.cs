using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DragonLib.GLTF
{
    [PublicAPI]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GLTFCameraType
    {
        Perspective,
        Orthographic
    }
}
