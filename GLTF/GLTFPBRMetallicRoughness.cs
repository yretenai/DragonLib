using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFPBRMetallicRoughness : GLTFProperty
    {
        public Vector4? BaseColorFactor { get; set; }
        public GLTFTextureInfo BaseColorTexture { get; set; }
        public float? MetallicFactor { get; set; }
        public float? RoughnessFactor { get; set; }
        public GLTFTextureInfo MetallicRoughnessTexture { get; set; }
    }
}
