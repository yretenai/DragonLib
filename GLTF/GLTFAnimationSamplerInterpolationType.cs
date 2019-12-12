using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI, JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GLTFAnimationSamplerInterpolationType
    {
        [EnumMember(Value = "LINEAR")]
        Linear,
        [EnumMember(Value = "STEP")]
        Step,
        // ReSharper disable once StringLiteralTypo
        [EnumMember(Value = "CATMULLROMSPLINE")]
        CatmullRomSpline,
        // ReSharper disable once StringLiteralTypo
        [EnumMember(Value = "CUBICSPLINE")]
        CubicSpline
    }
}
