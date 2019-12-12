using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI, JsonConverter(typeof(JsonStringEnumConverter))]
    public enum KHRLightsPunctualType
    {
        [EnumMember(Value = "directional")]
        Directional,
        [EnumMember(Value = "point")]
        Point,
        [EnumMember(Value = "spot")]
        Spot
    }
}
