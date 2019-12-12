using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAnimation : GLTFRootProperty
    {
        public List<GLTFAnimationChannel> Channels { get; set; } = new List<GLTFAnimationChannel>();
        public List<GLTFAnimationSampler> Samplers { get; set; } = new List<GLTFAnimationSampler>();
    }
}
