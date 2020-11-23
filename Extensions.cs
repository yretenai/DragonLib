using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DragonLib.Numerics;
using FlatBuffers;
using JetBrains.Annotations;

namespace DragonLib
{
    [PublicAPI]
    public static class Extensions
    {
        private static readonly sbyte[] SignedNibbles = { 0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1 };

        private static string[] BytePoints = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };
        
        public static ByteBuffer ToByteBuffer(this Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return new ByteBuffer(bytes);
        }

        public static ByteBuffer ToByteBuffer<T>(this Span<T> span) where T : struct => new ByteBuffer(MemoryMarshal.AsBytes(span).ToArray());

        public static string SanitizeFilename(this string path)
        {
            var illegal = Path.GetInvalidFileNameChars();

            return illegal.Aggregate(path, (current, ch) => current.Replace(ch, '?'));
        }

        public static string SanitizeDirname(this string path)
        {
            var illegal = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Distinct();

            return illegal.Aggregate(path, (current, ch) => current.Replace(ch, '_'));
        }

        public static string? ReadString(this Span<byte> data, Encoding? encoding = null)
        {
            if (data.Length == 0 || data[0] == 0) return null;
            var index = data.IndexOf<byte>(0);
            if (index <= -1) index = data.Length;

            return (encoding ?? Encoding.UTF8).GetString(data.Slice(0, index));
        }

        public static string ReadStringNonNull(this Span<byte> data, Encoding? encoding = null) => ReadString(data, encoding) ?? string.Empty;

        public static int Align(this int value, int n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value + (n - value % n);
        }

        public static uint Align(this uint value, uint n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value + (n - value % n);
        }

        public static long Align(this long value, long n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value + (n - value % n);
        }

        public static ulong Align(this ulong value, ulong n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value + (n - value % n);
        }

        public static int AlignReverse(this int value, int n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value - value % n;
        }

        public static uint AlignReverse(this uint value, uint n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value - value % n;
        }

        public static long AlignReverse(this long value, long n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value - value % n;
        }

        public static ulong AlignReverse(this ulong value, ulong n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value - value % n;
        }

        public static Span<byte> ToSpan(this string str, Encoding? encoding = null, bool endNull = false)
        {
            var bytes = (encoding ?? Encoding.UTF8).GetBytes(str);
            if (!endNull) return bytes;

            var span = new Span<byte>(new byte[bytes.Length + 1]);
            bytes.CopyTo(span);
            return span;
        }

        public static Span<byte> ToSpan(this Stream stream)
        {
            if (stream.Length - stream.Position == 0) return Span<byte>.Empty;
            var buffer = new Span<byte>(new byte[stream.Length - stream.Position]);
            stream.Read(buffer);
            return buffer;
        }

        public static string UnixPath(this string path, bool isDir)
        {
            var p = path.Replace('\\', '/');
            return isDir ? p + "/" : p;
        }

        public static string[] ToHexOctetsA(this string? input)
        {
            var cleaned = input?.Replace(" ", string.Empty).Trim();
            if (cleaned == null || cleaned.Length % 2 != 0) return new string[] { };

            return Enumerable.Range(0, cleaned.Length).Where(x => x % 2 == 0).Select(x => cleaned.Substring(x, 2)).ToArray();
        }

        public static string ToHexString(this Span<byte> input) => string.Join("", input.ToArray().Select(x => x.ToString("X2") + " "));

        public static byte[] ToHexOctetsB(this string? input)
        {
            var cleaned = input?.Replace(" ", string.Empty).Trim();
            if (cleaned == null || cleaned.Length % 2 != 0) return new byte[] { };

            return Enumerable.Range(0, cleaned.Length).Where(x => x % 2 == 0).Select(x => byte.Parse(cleaned.Substring(x, 2), NumberStyles.HexNumber)).ToArray();
        }

        public static int FindPointerFromSignature(this Span<byte> buffer, string signatureTemplate)
        {
            var signatureOctets = signatureTemplate.ToHexOctetsA();
            if (signatureOctets.Length < 1) return -1;
            var signature = signatureOctets.Select(x => x == "??" ? (byte?) null : Convert.ToByte(x, 16)).ToArray();
            for (var ptr = 0; ptr < buffer.Length - signature.Length; ++ptr)
            {
                var slice = buffer.Slice(ptr, signature.Length);
                var found = true;
                for (var i = 0; i < signature.Length; ++i)
                {
                    var b = signature[i];
                    if (b != null && b != slice[i]) found = false;
                }

                if (found) return ptr;
            }

            return -1;
        }

        public static int FindPointerFromSignatureReverse(this Span<byte> buffer, string signatureTemplate, int start = -1)
        {
            var signatureOctets = signatureTemplate.ToHexOctetsA();
            if (signatureOctets.Length < 1) return -1;
            var signature = signatureOctets.Select(x => x == "??" ? (byte?) null : Convert.ToByte(x, 16)).ToArray();
            if (start == -1 || start + signature.Length > buffer.Length) start = buffer.Length - signature.Length;
            for (var ptr = start; ptr > 0; --ptr)
            {
                var slice = buffer.Slice(ptr, signature.Length);
                var found = true;
                for (var i = 0; i < signature.Length; ++i)
                {
                    var b = signature[i];
                    if (b != null && b != slice[i]) found = false;
                }

                if (found) return ptr;
            }

            return -1;
        }

        public static int FindFirstAlphanumericSequence(this Span<byte> buffer, int length = 1)
        {
            for (var ptr = 0; ptr < buffer.Length - length; ++ptr)
            {
                var slice = buffer.Slice(ptr, length);
                if (slice.ToArray().All(x => x >= 0x30 && x <= 0x7F)) return ptr;
            }

            return -1;
        }

        // https://stackoverflow.com/questions/15743192/check-if-number-is-prime-number
        public static bool IsPrime(this int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (int) Math.Floor(Math.Sqrt(number));

            for (var i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Divides value by divisor and rounds up
        /// </summary>
        /// <param name="value"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static int DivideByRoundUp(this int value, int divisor) => (int) Math.Ceiling((double) value / divisor);

        /// <summary>
        ///     Constraints value by Minimum and Maximum short values
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static short ShortClamp(this int value)
        {
            if (value > short.MaxValue)
                return short.MaxValue;
            if (value < short.MinValue)
                return short.MinValue;
            return (short) value;
        }

        /// <summary>
        ///     Gets the higher 4 bits
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte GetHighNibble(this byte value) => (byte) (value >> 4 & 0xF);

        /// <summary>
        ///     Gets the lower 4 bits
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte GetLowNibble(this byte value) => (byte) (value & 0xF);

        /// <summary>
        ///     Gets the higher 4 bits, signed
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static sbyte GetHighNibbleSigned(this byte value) => SignedNibbles[value.GetHighNibble()];

        /// <summary>
        ///     Gets the lower 4 bits, signed
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static sbyte GetLowNibbleSigned(this byte value) => SignedNibbles[value.GetLowNibble()];

        /// <summary>
        ///     Returns human readable (12.3 GiB) format of a string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetHumanReadableBytes(this ulong bytes)
        {
            for (var i = 0; i < BytePoints.Length; ++i)
            {
                var divisor = Math.Pow(0x400, i);
                var nextDivisor = Math.Pow(0x400, i + 1);
                if (!(bytes < nextDivisor) && i != BytePoints.Length - 1) continue;
                var normalized = Math.Floor(bytes / (divisor / 10)) / 10;
                return $"{normalized} {BytePoints[i]}";
            }

            return $"{bytes} B";
        }

        #region System.Numerics

        public static Matrix4x4 ToDragon(this System.Numerics.Matrix4x4 matrix) => new Matrix4x4(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44);

        public static Vector2 ToDragon(this System.Numerics.Vector2 vector) => new Vector2(vector.X, vector.Y);

        public static Vector3 ToDragon(this System.Numerics.Vector3 vector) => new Vector3(vector.X, vector.Y, vector.Z);


        public static Vector4 ToDragon(this System.Numerics.Vector4 vector) => new Vector4(vector.X, vector.Y, vector.Z);

        public static Quaternion ToDragon(this System.Numerics.Quaternion quaternion) => new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

        #endregion
    }
}
