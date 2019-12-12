using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFNormalTextureInfo : GLTFTextureInfo
    {
        public float? Scale { get; set; }
    }
}
