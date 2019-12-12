using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAccessorSparseIndices : GLTFAccessorSparseValues
    {
        public GLTFComponentType ComponentType { get; set; }
    }
}
