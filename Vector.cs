using System.Linq;
using System.Numerics;

namespace DragonLib
{
    public struct Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector(params float[] values)
        {
            X = values.ElementAtOrDefault(0);
            Y = values.ElementAtOrDefault(1);
            Z = values.ElementAtOrDefault(2);
        }

        public (float x, float y, float z) ToTuple() => (X, Y, Z);

        public Vector<float> ToNumerics() => new Vector<float>(new[] { X, Y, Z });
    }
}
