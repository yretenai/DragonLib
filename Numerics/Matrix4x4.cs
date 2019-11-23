using System.Linq;
using JetBrains.Annotations;
using OpenTK;

namespace DragonLib.Numerics
{
    [PublicAPI]
    public struct Matrix4x4
    {
        public float M11 { get; }
        public float M12 { get; }
        public float M13 { get; }
        public float M14 { get; }
        public float M21 { get; }
        public float M22 { get; }
        public float M23 { get; }
        public float M24 { get; }
        public float M31 { get; }
        public float M32 { get; }
        public float M33 { get; }
        public float M34 { get; }
        public float M41 { get; }
        public float M42 { get; }
        public float M43 { get; }
        public float M44 { get; }

        public Matrix4x4(params float[] values)
        {
            M11 = values.ElementAtOrDefault(0);
            M12 = values.ElementAtOrDefault(1);
            M13 = values.ElementAtOrDefault(2);
            M14 = values.ElementAtOrDefault(3);
            M21 = values.ElementAtOrDefault(4);
            M22 = values.ElementAtOrDefault(5);
            M23 = values.ElementAtOrDefault(6);
            M24 = values.ElementAtOrDefault(7);
            M31 = values.ElementAtOrDefault(8);
            M32 = values.ElementAtOrDefault(9);
            M33 = values.ElementAtOrDefault(10);
            M34 = values.ElementAtOrDefault(11);
            M41 = values.ElementAtOrDefault(12);
            M42 = values.ElementAtOrDefault(13);
            M43 = values.ElementAtOrDefault(14);
            M44 = values.ElementAtOrDefault(15);
        }

        public Matrix4 ToOpenTK() => new Matrix4(M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);

        public System.Numerics.Matrix4x4 ToNumerics() => new System.Numerics.Matrix4x4(M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
    }
}
