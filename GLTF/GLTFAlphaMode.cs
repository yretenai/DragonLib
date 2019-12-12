using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI, JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GLTFAlphaMode
    {
        [EnumMember(Value = "OPAQUE")]
        Opaque,
        [EnumMember(Value = "MASK")]
        Mask,
        [EnumMember(Value = "BLEND")]
        Blend
    }
}
