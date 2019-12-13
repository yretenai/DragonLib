using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFCameraPerspective : GLTFProperty
    {
        public double AspectRatio { get; set; }

        [JsonPropertyName("yfov")]
        public double YFov { get; set; }

        [JsonPropertyName("zfar")]
        public double ZFar { get; set; } = double.PositiveInfinity;

        [JsonPropertyName("znear")]
        public double ZNear { get; set; }
    }
}
