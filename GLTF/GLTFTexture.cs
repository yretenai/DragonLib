using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFTexture : GLTFProperty
    {
        public int Sampler { get; set; }
        public int Source { get; set; }
    }
}
