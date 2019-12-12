using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAnimationChannelTarget : GLTFProperty
    {
        public int Node { get; set; }
        public GLTFAnimationChannelPath Path { get; set; }
    }
}
