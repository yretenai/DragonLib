using System;
using System.Text;
using DragonLib.Numerics;
using OpenTK;
using Matrix4x3 = DragonLib.Numerics.Matrix4x3;
using Quaternion = DragonLib.Numerics.Quaternion;
using Vector2 = DragonLib.Numerics.Vector2;
using Vector3 = DragonLib.Numerics.Vector3;
using Vector4 = DragonLib.Numerics.Vector4;

namespace DragonLib
{
    public static class Extensions
    {
        public static string ReadString(this Span<byte> data, Encoding encoding = null)
        {
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

        #region OpenTK Math

        public static Matrix4x4 ToDragon(this Matrix4 matrix)
        {
            return new Matrix4x4(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }

        public static Matrix4x3 ToDragon(this OpenTK.Matrix4x3 matrix)
        {
            return new Matrix4x3(matrix.M11, matrix.M12, matrix.M13, matrix.M21, matrix.M22, matrix.M23, matrix.M31, matrix.M32, matrix.M33, matrix.M41, matrix.M42, matrix.M43);
        }

        public static Matrix3x3 ToDragon(this Matrix3 matrix)
        {
            return new Matrix3x3(matrix.M11, matrix.M12, matrix.M13, matrix.M21, matrix.M22, matrix.M23, matrix.M31, matrix.M32, matrix.M33);
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
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        #endregion

        #region System.Numerics

        public static Matrix4x4 ToDragon(this System.Numerics.Matrix4x4 matrix)
        {
            return new Matrix4x4(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44);
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
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        #endregion
    }
}
