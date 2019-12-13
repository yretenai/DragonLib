using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class MSFTPackingNormalRoughnessMetallic : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "MSFT_packing_normalRoughnessMetallic";

        public int NormalRoughnessMetallicTexture { get; set; }

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFMaterial)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
