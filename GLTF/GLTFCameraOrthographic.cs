using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFCameraOrthographic : GLTFProperty
    {
        [JsonPropertyName("xmag")]
        public double XMag { get; set; }
        [JsonPropertyName("ymag")]
        public double YMag { get; set; }
        [JsonPropertyName("zfar")]
        public double ZFar { get; set; } = double.PositiveInfinity;
        [JsonPropertyName("znear")]
        public double ZNear { get; set; }
    }
}
