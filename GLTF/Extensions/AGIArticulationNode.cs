using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class AGIArticulationNode : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "AGI_articulations";
        
        public bool? IsAttachPoint { get; set; }
        public string ArticulationName { get; set; }
        
        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFRoot)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
