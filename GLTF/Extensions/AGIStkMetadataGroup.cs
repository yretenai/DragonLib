using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class AGIStkMetadataGroup : GLTFProperty
    {
        public string? Name { get; set; }
        public float? Efficiency { get; set; }
    }
}
