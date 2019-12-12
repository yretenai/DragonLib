using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class AGIStkMetadataNode : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "AGI_stk_metadata";

        public string Name { get; set; }
        public bool NoObscuration { get; set; } 
        
        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFNode)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
