using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class FBGeometryMetadataSceneBounds : GLTFProperty
    {
        public Vector3? Min { get; set; }
        public Vector3? Max { get; set; }
    }
}
