using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFMaterialCommonConstant : GLTFProperty
    {
        public Vector4? AmbientFactor { get; set; }
        public GLTFTextureInfo LightmapTexture { get; set; }
        public Vector4? LightmapFactor { get; set; }
    }
}
