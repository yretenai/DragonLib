using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFMaterial : GLTFRootProperty
    {
        public GLTFPBRMetallicRoughness PbrMetallicRoughness { get; set; }
        public GLTFMaterialCommonConstant CommonConstant { get; set; }
        public GLTFTextureInfo NormalTexture { get; set; }
        public GLTFTextureInfo OcclusionTexture { get; set; }
        public GLTFTextureInfo EmissiveTexture { get; set; }
        public Vector3? EmissiveFactor { get; set; }
        public GLTFAlphaMode? AlphaMode { get; set; }
        public float? AlphaCutoff { get; set; }
        public bool DoubleSided { get; set; }
    }
}
