using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFMeshPrimitive : GLTFProperty
    {
        public Dictionary<string, int> Attributes { get; set; } = new Dictionary<string, int>();
        public int Indices { get; set; }
        public int? Material { get; set; }
        public GLTFDrawMode Mode { get; set; } = GLTFDrawMode.Triangles;
        public List<Dictionary<string, int>> Targets { get; set; } = new List<Dictionary<string, int>>();
    }
}
