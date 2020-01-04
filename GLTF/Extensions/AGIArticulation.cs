using System.Collections.Generic;
using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class AGIArticulation : GLTFProperty
    {
        public string? Name { get; set; }
        public List<AGIArticulationStage>? Stages { get; set; }
        public Vector3? PointingVector { get; set; }
    }
}
