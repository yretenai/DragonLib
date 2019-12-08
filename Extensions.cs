using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DragonLib.Numerics;
using FlatBuffers;
using JetBrains.Annotations;
using OpenTK;
using Matrix4x3 = DragonLib.Numerics.Matrix4x3;
using Quaternion = DragonLib.Numerics.Quaternion;
using Vector2 = DragonLib.Numerics.Vector2;
using Vector3 = DragonLib.Numerics.Vector3;
using Vector4 = DragonLib.Numerics.Vector4;

namespace DragonLib
{
    [PublicAPI]
    public static class Extensions
    {
        public static ByteBuffer ToByteBuffer(this Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return new ByteBuffer(bytes);
        }

        public static ByteBuffer ToByteBuffer<T>(this Span<T> span) where T : struct
        {
            return new ByteBuffer(MemoryMarshal.AsBytes(span).ToArray());
        }

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

        public static string ReadString(this Span<byte> data, Encoding encoding = null)
        {
            if (data.Length == 0 || data[0] == 0) return null;
            var index = data.IndexOf<byte>(0);
            if (index <= -1) index = data.Length;

            return (encoding ?? Encoding.UTF8).GetString(data.Slice(0, index));
        }

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

        public static Span<byte> ToSpan(this Stream stream)
        {
            if (stream.Length - stream.Position == 0) return Span<byte>.Empty;
            var buffer = new Span<byte>(new byte[stream.Length - stream.Position]);
            stream.Read(buffer);
            return buffer;
        }

        public static string[] ToHexOctets(this string input)
        {
            var cleaned = input?.Replace(" ", "").Trim();
            if (cleaned == null || cleaned.Length % 2 != 0) return null;

            return Enumerable.Range(0, cleaned.Length).Where(x => x % 2 == 0).Select(x => cleaned.Substring(x, 2)).ToArray();
        }

        public static int FindPointerFromSignature(this Span<byte> buffer, string signatureTemplate)
        {
            var signatureOctets = signatureTemplate.ToHexOctets();
            if (signatureOctets == null || signatureOctets.Length < 1) return -1;
            var signature = signatureOctets.Select(x => x == "??" ? (byte?) null : Convert.ToByte(x, 16)).ToArray();
            for (var ptr = 0; ptr < buffer.Length - signature.Length; ++ptr)
            {
                var slice = buffer.Slice(ptr, signature.Length);
                var found = true;
                for (var i = 0; i < signature.Length; ++i)
                {
                    var b = signature[i];
                    if (b.HasValue && b.Value != slice[i]) found = false;
                }

                if (found) return ptr;
            }

            return -1;
        }

        public static int FindPointerFromSignatureReverse(this Span<byte> buffer, string signatureTemplate, int start = -1)
        {
            var signatureOctets = signatureTemplate.ToHexOctets();
            if (signatureOctets == null || signatureOctets.Length < 1) return -1;
            var signature = signatureOctets.Select(x => x == "??" ? (byte?) null : Convert.ToByte(x, 16)).ToArray();
            if (start == -1 || start + signature.Length > buffer.Length) start = buffer.Length - signature.Length;
            for (var ptr = start; ptr > 0; --ptr)
            {
                var slice = buffer.Slice(ptr, signature.Length);
                var found = true;
                for (var i = 0; i < signature.Length; ++i)
                {
                    var b = signature[i];
                    if (b.HasValue && b.Value != slice[i]) found = false;
                }

                if (found) return ptr;
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

            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }

        #region OpenTK Math

        public static Matrix4x4 ToDragon(this Matrix4 matrix)
        {
            return new Matrix4x4(matrix.M11, matrix.M12, matrix.M13,
                matrix.M14, matrix.M21, matrix.M22,
                matrix.M23, matrix.M24, matrix.M31,
                matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43,
                matrix.M44);
        }

        public static Matrix4x3 ToDragon(this OpenTK.Matrix4x3 matrix)
        {
            return new Matrix4x3(matrix.M11, matrix.M12, matrix.M13,
                matrix.M21, matrix.M22, matrix.M23,
                matrix.M31, matrix.M32, matrix.M33,
                matrix.M41, matrix.M42, matrix.M43);
        }

        public static Matrix3x3 ToDragon(this Matrix3 matrix)
        {
            return new Matrix3x3(matrix.M11, matrix.M12, matrix.M13,
                matrix.M21, matrix.M22, matrix.M23,
                matrix.M31, matrix.M32, matrix.M33);
        }

        public static Vector2 ToDragon(this OpenTK.Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector3 ToDragon(this OpenTK.Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }


        public static Vector4 ToDragon(this OpenTK.Vector4 vector)
        {
            return new Vector4(vector.X, vector.Y, vector.Z);
        }

        public static Quaternion ToDragon(this OpenTK.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                quaternion.W);
        }

        #endregion

        #region System.Numerics

        public static Matrix4x4 ToDragon(this System.Numerics.Matrix4x4 matrix)
        {
            return new Matrix4x4(matrix.M11, matrix.M12, matrix.M13,
                matrix.M14, matrix.M21, matrix.M22,
                matrix.M23, matrix.M24, matrix.M31,
                matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43,
                matrix.M44);
        }

        public static Vector2 ToDragon(this System.Numerics.Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector3 ToDragon(this System.Numerics.Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }


        public static Vector4 ToDragon(this System.Numerics.Vector4 vector)
        {
            return new Vector4(vector.X, vector.Y, vector.Z);
        }

        public static Quaternion ToDragon(this System.Numerics.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                quaternion.W);
        }

        #endregion
    }
}
