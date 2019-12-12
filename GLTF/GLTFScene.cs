using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFScene : GLTFRootProperty
    {
        public List<int> Nodes { get; set; } = new List<int>();
    }
}
