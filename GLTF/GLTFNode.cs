using System.Collections.Generic;
using DragonLib.Numerics;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFNode : GLTFRootProperty
    {
        public bool? UseTRS { get; set; }
        public int? Camera { get; set; }
        public List<int> Children { get; set; } = new List<int>();
        public int? Skin { get; set; }
        public Matrix4x4? Matrix { get; set; }
        public int? Mesh { get; set; }
        public Quaternion? Rotation { get; set; }
        public Vector3? Scale { get; set; }
        public Vector3? Translation { get; set; }
        public Dictionary<string, IGLTFExtension> Extras { get; set; } = new Dictionary<string, IGLTFExtension>();
    }
}
