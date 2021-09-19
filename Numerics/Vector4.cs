using System.Linq;

namespace DragonLib.Numerics
{
    public struct Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vector4(params float[] values)
        {
            X = values.ElementAtOrDefault(0);
            Y = values.ElementAtOrDefault(1);
            Z = values.ElementAtOrDefault(2);
            W = values.ElementAtOrDefault(3);
        }

        public System.Numerics.Vector4 ToNumerics() => new(X, Y, Z, W);
        public float[] ToArray() => new[] { X, Y, Y, Z };
    }
}
