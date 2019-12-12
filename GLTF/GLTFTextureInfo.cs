using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFTextureInfo : GLTFProperty
    {
        public int Index { get; set; }
        public int TexCoord { get; set; }
    }
}
