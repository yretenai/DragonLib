using System.Linq;

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

        public System.Numerics.Vector<float> ToNumerics() => new System.Numerics.Vector<float>(new float[3] { X, Y, Z });
    }
}
