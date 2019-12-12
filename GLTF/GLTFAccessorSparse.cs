using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAccessorSparse : GLTFProperty
    {
        public int Count { get; set; }
        public GLTFAccessorSparseIndices Indices { get; set; }
        public GLTFAccessorSparseValues Values { get; set; }
    }
}
