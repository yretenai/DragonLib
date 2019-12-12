using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFScene : GLTFProperty
    {
        public List<int> Nodes { get; set; } = new List<int>();
    }
}
