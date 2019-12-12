using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class KHRLightsPunctualNode : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "KHR_lights_punctual";
        
        public int Light { get; set; }
        
        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFNode)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
            root.ExtensionsRequired.Add(Identifier);
        }
    }
}
