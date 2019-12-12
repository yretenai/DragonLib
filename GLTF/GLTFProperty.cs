using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFProperty
    {
        public Dictionary<string, IGLTFExtension> Extensions { get; set; } = new Dictionary<string, IGLTFExtension>();
    }
}
