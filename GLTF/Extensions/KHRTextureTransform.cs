using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class KHRTextureTransform : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "KHR_texture_transform";

        public Vector2? Offset { get; set; }
        public float? Rotation { get; set; }
        public Vector2? Scale { get; set; }
        public int? TexCoord { get; set; }

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFTextureInfo)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
