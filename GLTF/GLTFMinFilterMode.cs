using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public enum GLTFMinFilterMode
    {
        None = 0,
        Nearest = 9728,
        Linear = 9729,
        NearestMipmapNearest = 9984,
        LinearMipmapNearest = 9985,
        NearestMipmapLinear = 9986,
        LinearMipmapLinear = 9987
    }
}
