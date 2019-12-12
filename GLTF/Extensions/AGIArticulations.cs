using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class AGIArticulations : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "AGI_articulations";
        
        public List<AGIArticulation> Articulations { get; set; } = new List<AGIArticulation>();
        
        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFRoot)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
