using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFSampler : GLTFRootProperty
    {
        public GLTFMagFilterMode MagFilter { get; set; } = GLTFMagFilterMode.Linear;
        public GLTFMinFilterMode MinFilter { get; set; } = GLTFMinFilterMode.NearestMipmapLinear;
        public GLTFWrapMode WrapS { get; set; } = GLTFWrapMode.Repeat;
        public GLTFWrapMode WrapT { get; set; } = GLTFWrapMode.Repeat;
    }
}
