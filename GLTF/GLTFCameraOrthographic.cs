using JetBrains.Annotations;
using Newtonsoft.Json;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFCameraOrthographic : GLTFProperty
    {
        [JsonProperty("xmag")]
        public double XMag { get; set; }

        [JsonProperty("ymag")]
        public double YMag { get; set; }

        [JsonProperty("zfar")]
        public double ZFar { get; set; } = double.PositiveInfinity;

        [JsonProperty("znear")]
        public double ZNear { get; set; }
    }
}
