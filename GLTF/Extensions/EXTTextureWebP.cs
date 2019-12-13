using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class EXTTextureWebP : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "EXT_texture_webp";

        public int Source { get; set; }

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFTexture)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
