using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public enum GLTFDrawMode
    {
        Points = 0,
        Lines = 1,
        LineLoop = 2,
        LineStrip = 3,
        Triangles = 4,
        TriangleStrip = 5,
        TriangleFan = 6
    }
}
