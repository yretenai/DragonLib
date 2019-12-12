using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFOcclusionTextureInfo : GLTFTextureInfo
    {
        public float? Strength { get; set; }
    }
}
