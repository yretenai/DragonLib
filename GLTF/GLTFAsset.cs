using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFAsset : GLTFProperty
    {
        public string? Copyright { get; set; }
        public string? Generator { get; set; }
        public string? Version { get; set; } = "2.0";
        public string? MinVersion { get; set; } = "2.0";
    }
}
