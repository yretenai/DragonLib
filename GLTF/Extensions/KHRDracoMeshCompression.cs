using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class KHRDracoMeshCompression : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "KHR_draco_mesh_compression";

        public int BufferVIew { get; set; }
        public Dictionary<string, int> Attributes { get; set; } = new Dictionary<string, int>();

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFMeshPrimitive)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
            root.ExtensionsRequired.Add(Identifier);
        }
    }
}
