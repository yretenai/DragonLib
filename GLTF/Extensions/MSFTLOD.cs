using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class MSFTLOD : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "MSFT_lod";

        public List<int> Ids { get; set; } = new List<int>();

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFMaterial) && !(gltf is GLTFNode)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
