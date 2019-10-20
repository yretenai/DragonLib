using System.Linq;

namespace DragonLib
{
    public struct Matrix4x3
    {
        public float M11 { get; }
        public float M12 { get; }
        public float M13 { get; }
        public float M21 { get; }
        public float M22 { get; }
        public float M23 { get; }
        public float M31 { get; }
        public float M32 { get; }
        public float M33 { get; }
        public float M41 { get; }
        public float M42 { get; }
        public float M43 { get; }

        public Matrix4x3(params float[] values)
        {
            M11 = values.ElementAtOrDefault(0);
            M12 = values.ElementAtOrDefault(1);
            M13 = values.ElementAtOrDefault(2);
            M21 = values.ElementAtOrDefault(3);
            M22 = values.ElementAtOrDefault(4);
            M23 = values.ElementAtOrDefault(5);
            M31 = values.ElementAtOrDefault(6);
            M32 = values.ElementAtOrDefault(7);
            M33 = values.ElementAtOrDefault(8);
            M41 = values.ElementAtOrDefault(9);
            M42 = values.ElementAtOrDefault(10);
            M43 = values.ElementAtOrDefault(11);
        }

        public float[] ToArray() => new float[12]
            {
                M11, M12, M13,
                M21, M22, M23,
                M31, M32, M33,
                M41, M42, M43
            };

        public System.Numerics.Matrix4x4 ToNumerics() => new System.Numerics.Matrix4x4(M11, M12, M13, 0, M21, M22, M23, 0, M31, M32, M33, 0, M41, M42, M43, 0);
    }
}
