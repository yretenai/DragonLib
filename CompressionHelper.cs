using System;
using System.IO;
using System.IO.Compression;
using LZ4;

namespace DragonLib
{
    public static class CompressionHelper
    {
        public static unsafe Span<byte> DecompressDEFLATE(Span<byte> data, int size)
        {
            fixed (byte* pinData = &data.GetPinnableReference())
            {
                using var stream = new UnmanagedMemoryStream(pinData, data.Length);
                using var inflateStream = new DeflateStream(stream, CompressionMode.Decompress);
                var block = new Span<byte>(new byte[size]);
                inflateStream.Read(block);
                return block;
            }
        }

        public static Span<byte> DecompressLZ4(Span<byte> data, int size)
        {
            return LZ4Codec.Decode(data.ToArray(), 0, data.Length, size);
        }
    }
}
