using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAnimation : GLTFProperty
    {
        public List<GLTFAnimationChannel> Channels { get; set; } = new List<GLTFAnimationChannel>();
        public List<GLTFAnimationSampler> Samplers { get; set; } = new List<GLTFAnimationSampler>();
    }
}
