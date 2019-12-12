using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAnimationChannel : GLTFProperty
    {
        public int Sampler { get; set; }
        public GLTFAnimationChannelTarget Target { get; set; }
    }
}
