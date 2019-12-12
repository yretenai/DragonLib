using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAccessorSparseValues : GLTFProperty
    {
        public int BufferView { get; set; }
        public int ByteOffset { get; set; }
    }
}
