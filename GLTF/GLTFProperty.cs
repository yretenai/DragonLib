using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFProperty
    {
        public Dictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();
    }
}
