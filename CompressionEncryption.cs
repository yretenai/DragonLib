using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using JetBrains.Annotations;
using LZ4;

namespace DragonLib
{
    [PublicAPI]
    public static class CompressionEncryption
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

        public static unsafe Span<byte> CryptAESCBC(Span<byte> data, Span<byte> key, Span<byte> iv)
        {
            if (iv.Length != 16 && iv.Length > 0)
            {
                var tmp = new Span<byte>(new byte[16]);
                iv.Slice(0, Math.Min(16, iv.Length)).CopyTo(tmp);
            }
            else if (iv.Length == 0)
            {
                iv = new byte[16];
            }

            using var aes = new AesManaged
            {
                Key = key.ToArray(),
                IV = iv.ToArray(),
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC
            };

            fixed (byte* pinData = &data.GetPinnableReference())
            {
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var stream = new UnmanagedMemoryStream(pinData, data.Length);
                using var crypto = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
                var block = new Span<byte>(new byte[data.Length]);
                var aligned = data.Length.AlignReverse(16);
                crypto.Read(block.Slice(0, aligned));
                if (aligned < block.Length) data.Slice(aligned).CopyTo(block.Slice(aligned));

                return block;
            }
        }

        // Source: QuickBMS - unz.c
        private static unsafe int UnsafeLZ77EA_970_Readnumber(byte** inSteam)
        {
            int total = 0, t;
            do
            {
                t = **inSteam;
                (*inSteam)++;
                total += t;
            } while (t == 0xff);

            return total;
        }

        // Source: QuickBMS - unz.c
        private static unsafe int UnsafeLZ77EA_970(byte* inSteam, int insz, byte* outStream, int outsz)
        {
            if (insz == 0) return 0;
            byte* inl = inSteam + insz;
            int ret = 0;
            while (inSteam < inl)
            {
                int lengthByte = *inSteam++;
                int proceedSize = lengthByte >> 4;
                int copySize = lengthByte & 0xf;

                if (proceedSize == 0xf) proceedSize += UnsafeLZ77EA_970_Readnumber(&inSteam);

                for (var i = 0; i < proceedSize; i++)
                {
                    if (ret >= outsz) break;
                    outStream[ret++] = *inSteam++;
                }

                if (inSteam >= inl) break;

                int offset = inSteam[0] | (inSteam[1] << 8);
                inSteam += 2;

                if (copySize == 0xf) copySize += UnsafeLZ77EA_970_Readnumber(&inSteam);
                copySize += 4;

                for (var i = 0; i < copySize; i++)
                {
                    if (ret >= outsz) break;
                    outStream[ret] = outStream[ret - offset];
                    ret++;
                }
            }

            return ret;
        }

        // This hurts my soul.
        public static int UnsafeDecompressLZ77EA_970(Span<byte> buffer, Span<byte> output)
        {
            unsafe
            {
                fixed (byte* inStream = &buffer.GetPinnableReference())
                fixed (byte* outStream = &output.GetPinnableReference())
                    return UnsafeLZ77EA_970(inStream, buffer.Length, outStream, output.Length);
            }
        }
    }
}
