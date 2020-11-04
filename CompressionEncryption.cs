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

        public static unsafe Span<byte> DecompressDEFLATEIonic(Span<byte> data, int size)
        {
            fixed (byte* pinData = &data.GetPinnableReference())
            {
                using var stream = new UnmanagedMemoryStream(pinData, data.Length);
                using var inflateStream = new Ionic.Zlib.DeflateStream(stream, Ionic.Zlib.CompressionMode.Decompress);
                var block = new Span<byte>(new byte[size]);
                inflateStream.Read(block);
                return block;
            }
        }
        public static Span<byte> CompressDEFLATE(Span<byte> data, int compressionLevel = 1)
        {
            using var stream = new MemoryStream();
            using var inflateStream = new DeflateStream(stream, (CompressionLevel) compressionLevel, true);
            inflateStream.Write(data);
            inflateStream.Flush();
            inflateStream.Close();
            var compressed = new Span<byte>(new byte[stream.Position]);
            stream.Position = 0;
            stream.Read(compressed);
            return compressed;
        }

        public static Span<byte> CompressDEFLATEIonic(Span<byte> data, int compressionLevel = 9)
        {
            using var stream = new MemoryStream();
            using var inflateStream = new Ionic.Zlib.DeflateStream(stream, Ionic.Zlib.CompressionMode.Compress, (Ionic.Zlib.CompressionLevel) compressionLevel, true);
            inflateStream.Write(data);
            inflateStream.Flush();
            inflateStream.Close();
            var compressed = new Span<byte>(new byte[stream.Position]);
            stream.Position = 0;
            stream.Read(compressed);
            return compressed;
        }

        public static Span<byte> DecompressLZ4(Span<byte> data, int size) => LZ4Codec.Decode(data.ToArray(), 0, data.Length, size);

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
                var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
                using var stream = new UnmanagedMemoryStream(pinData, data.Length);
                using var crypto = new CryptoStream(stream, decrypt, CryptoStreamMode.Read);
                var block = new Span<byte>(new byte[data.Length]);
                var aligned = data.Length.AlignReverse(16);
                crypto.Read(block.Slice(0, aligned));
                if (aligned < block.Length) data.Slice(aligned).CopyTo(block.Slice(aligned));

                return block;
            }
        }

        // https://github.com/Ryujinx/Ryujinx/blob/b2b736abc2569ab5d8199da666aef8d8394844a0/Ryujinx.HLE/Loaders/Compression/Lz4.cs
        // Adapted for Span<T>
        public static int DecompressLZ4(Span<byte> cmp, Span<byte> dec)
        {
            var cmpPos = 0;
            var decPos = 0;

            do
            {
                var token = cmp[cmpPos++];

                var encCount = token >> 0 & 0xf;
                var litCount = token >> 4 & 0xf;

                // Copy literal chunk
                if (litCount == 0xF)
                {
                    byte sum;
                    do
                    {
                        litCount += sum = cmp[cmpPos++];
                    } while (sum == 0xff);
                }

                cmp.Slice(cmpPos, litCount).CopyTo(dec.Slice(decPos, litCount));

                cmpPos += litCount;
                decPos += litCount;

                if (cmpPos >= cmp.Length) break;

                // Copy compressed chunk
                var back = cmp[cmpPos++] << 0 | cmp[cmpPos++] << 8;

                if (encCount == 0xF)
                {
                    byte sum;
                    do
                    {
                        encCount += sum = cmp[cmpPos++];
                    } while (sum == 0xff);
                }

                encCount += 4;

                var encPos = decPos - back;

                if (encCount <= back)
                {
                    dec.Slice(encPos, encCount).CopyTo(dec.Slice(decPos, encCount));

                    decPos += encCount;
                }
                else
                {
                    while (encCount-- > 0) dec[decPos++] = dec[encPos++];
                }
            } while (cmpPos < cmp.Length && decPos < dec.Length);

            return decPos;
        }
    }
}
