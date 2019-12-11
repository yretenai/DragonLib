using System.Linq;
using JetBrains.Annotations;
using OpenTK;

namespace DragonLib.Numerics
{
    [PublicAPI]
    public struct Matrix3x3
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

        public Matrix3x3(params float[] values)
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
        }

        public Matrix3 ToOpenTK() =>
            new Matrix3(M11, M12, M13, M21, M22, M23, M31, M32, M33);
    }
}
