using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI, JsonConverter(typeof(StringEnumConverter))]
    public enum KHRLightsPunctualType
    {
        Directional,
        Point,
        Spot
    }
}
