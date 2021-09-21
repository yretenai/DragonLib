using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace DragonLib.IO {
    public static class CompressionEncryption {
        public static unsafe Span<byte> DecompressDEFLATE(Span<byte> data, int size) {
            fixed (byte* pinData = &data.GetPinnableReference()) {
                using var stream = new UnmanagedMemoryStream(pinData, data.Length);
                using var inflateStream = new DeflateStream(stream, CompressionMode.Decompress);
                var block = new Span<byte>(new byte[size]);
                inflateStream.Read(block);
                return block;
            }
        }

        public static Span<byte> CompressDEFLATE(Span<byte> data, int compressionLevel = 1) {
            using var stream = new MemoryStream();
            using var inflateStream = new DeflateStream(stream, (CompressionLevel)compressionLevel, true);
            inflateStream.Write(data);
            inflateStream.Flush();
            inflateStream.Close();
            var compressed = new Span<byte>(new byte[stream.Position]);
            stream.Position = 0;
            stream.Read(compressed);
            return compressed;
        }

        public static unsafe Span<byte> CryptAESCBC(Span<byte> data, Span<byte> key, Span<byte> iv) {
            if (iv.Length != 16 && iv.Length > 0) {
                var tmp = new Span<byte>(new byte[16]);
                iv[..Math.Min(16, iv.Length)].CopyTo(tmp);
            } else if (iv.Length == 0) {
                iv = new byte[16];
            }

            using var aes = Aes.Create();
            aes.Key = key.ToArray();
            aes.IV = iv.ToArray();
            aes.Padding = PaddingMode.Zeros;
            aes.Mode = CipherMode.CBC;

            fixed (byte* pinData = &data.GetPinnableReference()) {
                var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
                using var stream = new UnmanagedMemoryStream(pinData, data.Length);
                using var crypto = new CryptoStream(stream, decrypt, CryptoStreamMode.Read);
                var block = new Span<byte>(new byte[data.Length]);
                var aligned = data.Length.AlignReverse(16);
                crypto.Read(block[..aligned]);
                if (aligned < block.Length) data[aligned..].CopyTo(block[aligned..]);

                return block;
            }
        }

        // https://github.com/Ryujinx/Ryujinx/blob/b2b736abc2569ab5d8199da666aef8d8394844a0/Ryujinx.HLE/Loaders/Compression/Lz4.cs
        // Adapted for Span<T>
        public static int DecompressLZ4(Span<byte> cmp, Span<byte> dec, bool swap = false) {
            var cmpPos = 0;
            var decPos = 0;

            do {
                var token = cmp[cmpPos++];

                var encCount = (token >> 0) & 0xf;
                var litCount = (token >> 4) & 0xf;
                if (swap) (encCount, litCount) = (litCount, encCount);

                // Copy literal chunk
                if (litCount == 0xF) {
                    byte sum;
                    do {
                        litCount += sum = cmp[cmpPos++];
                    } while (sum == 0xff);
                }

                cmp.Slice(cmpPos, litCount).CopyTo(dec.Slice(decPos, litCount));

                cmpPos += litCount;
                decPos += litCount;

                if (cmpPos >= cmp.Length) break;

                // Copy compressed chunk
                var back = (cmp[cmpPos++] << 0) | (cmp[cmpPos++] << 8);

                if (encCount == 0xF) {
                    byte sum;
                    do {
                        encCount += sum = cmp[cmpPos++];
                    } while (sum == 0xff);
                }

                encCount += 4;

                var encPos = decPos - back;

                if (encCount <= back) {
                    dec.Slice(encPos, encCount).CopyTo(dec.Slice(decPos, encCount));

                    decPos += encCount;
                } else {
                    while (encCount-- > 0) dec[decPos++] = dec[encPos++];
                }
            } while (cmpPos < cmp.Length && decPos < dec.Length);

            return decPos;
        }

        // Taken from Evolution Engine Cache Extractor, updated to use System.Buffer
        public static int DecompressLZF(ReadOnlySpan<byte> compressedData, Span<byte> decompressedData) {
            var compPos = 0;
            var decompPos = 0;

            while (compPos < compressedData.Length) {
                var codeWord = compressedData[compPos++];
                if (codeWord <= 0x1F) {
                    // Encode literal
                    for (int i = codeWord; i >= 0; --i) {
                        decompressedData[decompPos] = compressedData[compPos];
                        ++decompPos;
                        ++compPos;
                    }
                } else {
                    // Encode dictionary
                    var copyLen = codeWord >> 5; // High 3 bits are copy length
                    if (copyLen == 7) // If those three make 7, then there are more bytes to copy (maybe)
                        copyLen += compressedData[compPos++]; // Grab next byte and add 7 to it

                    var dictDist = ((codeWord & 0x1f) << 8) | compressedData[compPos]; // 13 bits code lookback offset
                    ++compPos;
                    copyLen += 2; // Add 2 to copy length

                    var decompDistBeginPos = decompPos - 1 - dictDist;

                    for (var i = 0; i < copyLen; ++i, ++decompPos) decompressedData[decompPos] = decompressedData[decompDistBeginPos + i];
                }
            }

            return decompPos;
        }
    }
}
