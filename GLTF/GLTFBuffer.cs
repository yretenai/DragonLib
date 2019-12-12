using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFBuffer : GLTFProperty
    {
        public string Uri { get; set; }
        public uint ByteLength { get; set; }
    }
}
