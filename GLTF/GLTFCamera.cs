using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFCamera : GLTFProperty
    {
        public GLTFCameraOrthographic Orthographic { get; set; }
        public GLTFCameraPerspective Perspective { get; set; }
        public GLTFCameraType Type { get; set; }
    }
}
