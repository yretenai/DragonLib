using System;
using System.Runtime.InteropServices;
using Squish;

namespace DragonLib.DXGI
{
    public static class DXGI
    {
        public static Span<byte> BuildDDS(DXGIPixelFormat pixel, int mipCount, int width, int height, Span<byte> blob)
        {
            var result = new Span<byte>(new byte[blob.Length + 0x94]);
            var header = new DDSImageHeader
            {
                Magic = 0x20534444,
                Size = 0x7C,
                Flags = 0x1 | 0x2 | 0x4 | 0x1000 | 0x20000,
                Width = width,
                Height = height,
                LinearSize = 0,
                Depth = 0,
                MipmapCount = mipCount,
                Format = new DDSPixelFormat
                {
                    Size = 0x20,
                    Flags = 4,
                    FourCC = 0x30315844,
                    BitCount = 0x20,
                    RedMask = 0x0000FF00,
                    GreenMask = 0x00FF0000,
                    BlueMask = 0xFF000000,
                    AlphaMask = 0x000000FF
                },
                Caps1 = 0x1008,
                Caps2 = 0,
                Caps3 = 0,
                Caps4 = 0,
                Reserved2 = 0
            };
            MemoryMarshal.Write(result, ref header);
            var dx10 = new DXT10Header
            {
                Format = (int)pixel,
                Dimension = DXT10ResourceDimension.TEXTURE2D,
                Misc = 0,
                Size = 1
            };
            MemoryMarshal.Write(result.Slice(0x80), ref dx10);
            blob.CopyTo(result.Slice(0x94));
            return result;
        }

        public static Span<byte> BGRARGBA(Span<byte> BGRA)
        {
            var value = new Span<byte>(new byte[BGRA.Length]);
            for (var i = 0; i < BGRA.Length; i += 4)
            {
                value[i + 0] = BGRA[i + 2];
                value[i + 1] = BGRA[i + 1];
                value[i + 2] = BGRA[i + 0];
                value[i + 3] = BGRA[i + 3];
            }
            return value;
        }

        public static Span<byte> DecompressDXGIFormat(Span<byte> data, int width, int height, DXGIPixelFormat format)
        {
            var req = width * height * 4;
            switch (format)
            {
                case DXGIPixelFormat.R8G8B8A8_SINT:
                case DXGIPixelFormat.R8G8B8A8_UINT:
                case DXGIPixelFormat.R8G8B8A8_UNORM:
                case DXGIPixelFormat.R8G8B8A8_UNORM_SRGB:
                case DXGIPixelFormat.R8G8B8A8_SNORM:
                case DXGIPixelFormat.R8G8B8A8_TYPELESS:
                    return data;
                case DXGIPixelFormat.B8G8R8A8_UNORM:
                case DXGIPixelFormat.B8G8R8A8_UNORM_SRGB:
                case DXGIPixelFormat.B8G8R8A8_TYPELESS:
                    return BGRARGBA(data);
                case DXGIPixelFormat.BC1_TYPELESS:
                case DXGIPixelFormat.BC1_UNORM:
                case DXGIPixelFormat.BC1_UNORM_SRGB:
                    {
                        var dec = new byte[req];
                        var dataArray = data.ToArray();
                        Squish.Squish.DecompressImage(dec, width, height, ref dataArray, SquishFlags.kDxt1);
                        return new Span<byte>(dec);
                    }
                case DXGIPixelFormat.BC2_TYPELESS:
                case DXGIPixelFormat.BC2_UNORM:
                case DXGIPixelFormat.BC2_UNORM_SRGB:
                    {
                        var dec = new byte[req];
                        var dataArray = data.ToArray();
                        Squish.Squish.DecompressImage(dec, width, height, ref dataArray, SquishFlags.kDxt3);
                        return new Span<byte>(dec);
                    }
                case DXGIPixelFormat.BC3_TYPELESS:
                case DXGIPixelFormat.BC3_UNORM:
                case DXGIPixelFormat.BC3_UNORM_SRGB:
                    {
                        var dec = new byte[req];
                        var dataArray = data.ToArray();
                        Squish.Squish.DecompressImage(dec, width, height, ref dataArray, SquishFlags.kDxt5);
                        return new Span<byte>(dec);
                    }
                default:
                    throw new InvalidOperationException($"Format {format} is not supported yet!");
            }
        }
    }
}
