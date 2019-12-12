using System.Collections.Generic;
using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class EXTLightsImageBasedLight : GLTFProperty
    {
        public Quaternion? Rotation { get; set; }
        public float? Intensity { get; set; }
        public List<Vector3?> IrradianceCoefficients { get; set; } = new List<Vector3?>();
        public List<List<int>> SpecularImages { get; set; } = new List<List<int>>();
        public int SpecularImageSize { get; set; }
    }
}
