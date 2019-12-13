using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class KHRMaterialsPBRSpecularGlossiness : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "KHR_materials_pbrSpecularGlossiness";

        public Vector4? DiffuseFactor { get; set; }
        public GLTFTextureInfo DiffuseTexture { get; set; }
        public Vector3? SpecularFactor { get; set; }
        public float? GlossinessFactor { get; set; }
        public GLTFTextureInfo SpecularGlossinessTexture { get; set; }

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFMaterial)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
            root.ExtensionsRequired.Add(Identifier);
        }
    }
}
