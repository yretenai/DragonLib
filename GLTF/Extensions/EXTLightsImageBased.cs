using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class EXTLightsImageBased : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "EXT_lights_image_based";

        public List<EXTLightsImageBasedLight> Lights { get; set; } = new List<EXTLightsImageBasedLight>();

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFScene)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
