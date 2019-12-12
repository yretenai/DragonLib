using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class MSFTPackingOcclusionRoughnessMetallic : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "MSFT_packing_occlusionRoughnessMetallic";
        
        public int OcclusionRoughnessMetallicTexture { get; set; }
        public int RoughnessMetallicOcclusionTexture { get; set; }
        public int NormalTexture { get; set; }
        
        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFMaterial)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
