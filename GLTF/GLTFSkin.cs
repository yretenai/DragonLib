using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFSkin : GLTFRootProperty
    {
        public int InverseBindMatrices { get; set; }
        public int Skeleton { get; set; }
        public List<int> Joints { get; set; } = new List<int>();
    }
}
