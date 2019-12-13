using JetBrains.Annotations;
using Newtonsoft.Json;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFCameraPerspective : GLTFProperty
    {
        public double AspectRatio { get; set; }

        [JsonProperty("yfov")]
        public double YFov { get; set; }

        [JsonProperty("zfar")]
        public double ZFar { get; set; } = double.PositiveInfinity;

        [JsonProperty("znear")]
        public double ZNear { get; set; }
    }
}
