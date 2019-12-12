using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAccessor : GLTFProperty
    {
        public int BufferView { get; set; }
        public uint ByteOffset { get; set; }
        public GLTFComponentType ComponentType { get; set; }
        public bool Normalized { get; set; }
        public uint Count { get; set; }
        public GLTFAccessorAttributeType Type { get; set; }
        public List<double> Max { get; set; } = new List<double>();
        public List<double> Min { get; set; } = new List<double>();
        public GLTFAccessorSparse Sparse { get; set; }
    }
}
