using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFImage : GLTFRootProperty
    {
        public string Uri { get; set; }
        public string MimeType { get; set; }
        public int? BufferView { get; set; }
    }
}
