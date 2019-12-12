using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI, JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AGIArticulationStageType
    {
        XTranslate,
        YTranslate,
        ZTranslate,
        XRotate,
        YRotate,
        ZRotate,
        XScale,
        YScale,
        ZScale,
        UniformScale,
    }
}
