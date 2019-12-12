using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAnimationSampler : GLTFProperty
    {
        public int Input { get; set; }
        public GLTFAnimationSamplerInterpolationType Interpolation { get; set; }
        public int Output { get; set; }
    }
}
