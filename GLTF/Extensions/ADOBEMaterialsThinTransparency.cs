using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class ADOBEMaterialsThinTransparency : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "ADOBE_materials_thin_transparency";

        public float? TransmissionFactor { get; set; }
        public GLTFTextureInfo TransmissionTexture { get; set; }

        [JsonPropertyName("ior")]
        public float? IOR { get; set; }

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFMaterial)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
            root.ExtensionsRequired.Add(Identifier);
        }
    }
}
