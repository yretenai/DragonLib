using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFTexture : GLTFRootProperty
    {
        public int Sampler { get; set; }
        public int Source { get; set; }
    }
}
