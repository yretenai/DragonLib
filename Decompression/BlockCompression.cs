using System;
using System.Runtime.InteropServices;

namespace DragonLib.Decompression
{
    // Translated from JS
    // https://github.com/jangxx/node-s3tc/blob/master/index.js
    // No license.
    public static class BlockCompression
    {
        public static Span<byte> DecompressBC3(Span<byte> data)
        {
            var blockCount = data.Length / 16;
            var decompressed = new Span<byte>(new byte[data.Length * 4]);
            for (var i = 0; i < blockCount; ++i)
            {
                var block = data.Slice(i * 16, 16);
                var pixel = i * 16 * 4;
                var alpha0 = block[0];
                var alpha1 = block[1];
                var alphaTable = block.Slice(2, 6);
                alphaTable.Reverse();
                var color0 = ToR8G8B8(MemoryMarshal.Read<ushort>(block.Slice(8)));
                var color1 = ToR8G8B8(MemoryMarshal.Read<ushort>(block.Slice(10)));
                var table = data.Slice(12);
                var alphaTableCorrected = new byte[16] {
                    (byte)(0x7 & (alphaTable[5] >> 0)),
                    (byte)(0x7 & (alphaTable[5] >> 3)),
                    (byte)(0x7 & (((0x1 & alphaTable[4]) << 2) + (alphaTable[5] >> 6))),
                    (byte)(0x7 & (alphaTable[4] >> 1)),
                    (byte)(0x7 & (alphaTable[4] >> 4)),
                    (byte)(0x7 & (((0x3 & alphaTable[3]) << 1) + (alphaTable[4] >> 7))),
                    (byte)(0x7 & (alphaTable[3] >> 2)),
                    (byte)(0x7 & (alphaTable[3] >> 5)),
                    (byte)(0x7 & (alphaTable[2] >> 0)),
                    (byte)(0x7 & (alphaTable[2] >> 3)),
                    (byte)(0x7 & (((0x1 & alphaTable[1]) << 2) + (alphaTable[2] >> 6))),
                    (byte)(0x7 & (alphaTable[1] >> 1)),
                    (byte)(0x7 & (alphaTable[1] >> 4)),
                    (byte)(0x7 & (((0x3 & alphaTable[0]) << 1) + (alphaTable[1] >> 7))),
                    (byte)(0x7 & (alphaTable[0] >> 2)),
                    (byte)(0x7 & (alphaTable[0] >> 5))
                };
                for (var j = 0; j < 16; ++j)
                {
                    var code = table[3 & (j / 4)];
                    decompressed[j + pixel + 0] = CalculateBC3Point(code, color0.R, color1.R);
                    decompressed[j + pixel + 1] = CalculateBC3Point(code, color0.G, color1.G);
                    decompressed[j + pixel + 2] = CalculateBC3Point(code, color0.B, color1.B);
                    decompressed[j + pixel + 3] = CalculateBC3Alpha(alphaTableCorrected[j], alpha0, alpha1);
                }
            }
            return decompressed;
        }

        private static byte CalculateBC3Point(byte code, byte color0, byte color1)
        {
            if (code == 0) return color0;
            if (code == 1) return color1;
            return (byte)((color0 + color1 + 1) >> 1);
        }

        private static byte CalculateBC3Alpha(byte code, byte alpha0, byte alpha1)
        {
            if (alpha0 > alpha1)
            {
                switch (code)
                {
                    case 0: return alpha0;
                    case 1: return alpha1;
                    case 2: return (byte)((6 * alpha0 + 1 * alpha1) / 7);
                    case 3: return (byte)((5 * alpha0 + 2 * alpha1) / 7);
                    case 4: return (byte)((4 * alpha0 + 3 * alpha1) / 7);
                    case 5: return (byte)((3 * alpha0 + 4 * alpha1) / 7);
                    case 6: return (byte)((2 * alpha0 + 5 * alpha1) / 7);
                    case 7: return (byte)((1 * alpha0 + 6 * alpha1) / 7);
                }
            }
            else
            {
                switch (code)
                {
                    case 0: return alpha0;
                    case 1: return alpha1;
                    case 2: return (byte)((4 * alpha0 + 1 * alpha1) / 5);
                    case 3: return (byte)((3 * alpha0 + 2 * alpha1) / 5);
                    case 4: return (byte)((2 * alpha0 + 3 * alpha1) / 5);
                    case 5: return (byte)((1 * alpha0 + 4 * alpha1) / 5);
                    case 6: return 0;
                    case 7: return 255;
                }
            }

            return 255;
        }

        public static Span<byte> DecompressBC1(Span<byte> data)
        {
            var blockCount = data.Length / 8;
            var decompressed = new Span<byte>(new byte[data.Length * 8]);
            for (var i = 0; i < blockCount; ++i)
            {
                var block = data.Slice(i * 8, 8);
                var pixel = i * 16 * 4;
                var color0 = ToR8G8B8(MemoryMarshal.Read<ushort>(block));
                var color1 = ToR8G8B8(MemoryMarshal.Read<ushort>(block.Slice(2)));
                var table = data.Slice(4);
                for (var j = 0; j < 16; ++j)
                {
                    var code = table[3 & (j / 4)];
                    decompressed[j + pixel + 0] = CalculateBC1Point(code, color0.R, color1.R);
                    decompressed[j + pixel + 1] = CalculateBC1Point(code, color0.G, color1.G);
                    decompressed[j + pixel + 2] = CalculateBC1Point(code, color0.B, color1.B);
                    decompressed[j + pixel + 3] = 0xFF;
                }
            }
            return decompressed;
        }

        private static byte CalculateBC1Point(byte code, byte color0, byte color1)
        {
            if (code == 0) return color0;
            if (code == 1) return color1;
            if (color0 > color1)
            {
                switch (code)
                {
                    case 2: return (byte)((2 * color0 + color1) / 3);
                    case 3: return (byte)((color0 + 2 * color1) / 3);
                }
            }

            return (byte)((color0 + color1 + 1) >> 1);
        }

        private static (byte R, byte G, byte B) ToR8G8B8(ushort rgb)
        {
            return ((byte)(((rgb & 0xF800) >> 11) * 8),
                    (byte)(((rgb & 0x07E0) >> 05) * 4),
                    (byte)(((rgb & 0x001F) >> 00) * 8));
        }
    }
}
