using System.Linq;

namespace DragonLib
{
    public struct Quaternion
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Quaternion(params float[] values)
        {
            X = values.ElementAtOrDefault(0);
            Y = values.ElementAtOrDefault(1);
            Z = values.ElementAtOrDefault(2);
            W = values.ElementAtOrDefault(3);
        }

        public (float x, float y, float z, float w) ToTuple() => (X, Y, Z, W);

        public System.Numerics.Quaternion ToNumerics() => new System.Numerics.Quaternion(X, Y, Z, W);
    }
}
