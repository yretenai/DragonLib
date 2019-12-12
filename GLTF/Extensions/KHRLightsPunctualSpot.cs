using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class KHRLightsPunctualSpot : GLTFProperty
    {
        public float? InnerConeAngle { get; set; }
        public float? OuterConeAngle { get; set; }
    }
}
