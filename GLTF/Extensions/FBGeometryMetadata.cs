using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class FBGeometryMetadata : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "FB_geometry_metadata";

        public float? VertexCount { get; set; }
        public float? PrimitiveCount { get; set; }
        public FBGeometryMetadataSceneBounds SceneBounds { get; set; }
        
        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFScene)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
