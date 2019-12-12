using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFMesh : GLTFProperty
    {
        public List<GLTFMeshPrimitive> Primitives { get; set; } = new List<GLTFMeshPrimitive>();
        public List<double> Weights { get; set; } = new List<double>();
    }
}
