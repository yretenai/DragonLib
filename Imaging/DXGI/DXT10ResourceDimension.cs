using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace DragonLib.Imaging.DXGI
{
    [PublicAPI]
    public enum DXT10ResourceDimension
    {
        UNKNOWN = 0,
        BUFFER = 1,
        TEXTURE1D = 2,
        TEXTURE2D = 3,
        TEXTURE3D = 4
    }
}
