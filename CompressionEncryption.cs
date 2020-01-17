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

            using var aes = new AesManaged()
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
                if (aligned < block.Length)
                {
                    data.Slice(aligned).CopyTo(block.Slice(aligned));
                }

                return block;
            }
        }
    }
}
