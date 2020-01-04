using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class KHRLightsPunctual : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "KHR_lights_punctual";

        public string? Name { get; set; }
        public Vector3? Color { get; set; }
        public int? Intensity { get; set; }
        public KHRLightsPunctualType Type { get; set; }
        public float? Range { get; set; }
        public KHRLightsPunctualSpot? Spot { get; set; }

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFRoot)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
            root.ExtensionsRequired.Add(Identifier);
        }
    }
}
