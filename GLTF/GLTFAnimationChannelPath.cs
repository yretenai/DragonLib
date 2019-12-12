using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI, JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GLTFAnimationChannelPath
    {
        [EnumMember(Value = "translation")]
        Translation,
        [EnumMember(Value = "rotation")]
        Rotation,
        [EnumMember(Value = "scale")]
        Scale,
        [EnumMember(Value = "weights")]
        Weights
    }
}
