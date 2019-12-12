using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI, JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AGIArticulationStageType
    {
        [EnumMember(Value = "xTranslate")]
        XTranslate,
        [EnumMember(Value = "yTranslate")]
        YTranslate,
        [EnumMember(Value = "zTranslate")]
        ZTranslate,
        [EnumMember(Value = "xRotate")]
        XRotate,
        [EnumMember(Value = "yRotate")]
        YRotate,
        [EnumMember(Value = "zRotate")]
        ZRotate,
        [EnumMember(Value = "xScale")]
        XScale,
        [EnumMember(Value = "yScale")]
        YScale,
        [EnumMember(Value = "zScale")]
        ZScale,
        [EnumMember(Value = "uniformScale")]
        UniformScale,
    }
}
