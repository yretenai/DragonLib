using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class AGIStkMetadata : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "AGI_stk_metadata";

        public List<AGIStkMetadataGroup> Groups { get; set; } = new List<AGIStkMetadataGroup>();
        
        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFRoot)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
