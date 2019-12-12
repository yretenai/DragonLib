using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFBuffer : GLTFRootProperty
    {
        public string Uri { get; set; }
        public uint ByteLength { get; set; }
    }
}
