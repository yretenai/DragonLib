using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum KHRLightsPunctualType
    {
        Directional,
        Point,
        Spot
    }
}
