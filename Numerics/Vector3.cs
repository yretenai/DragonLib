using System.Linq;

namespace DragonLib.Numerics
{
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(params float[] values)
        {
            X = values.ElementAtOrDefault(0);
            Y = values.ElementAtOrDefault(1);
            Z = values.ElementAtOrDefault(2);
        }

        public OpenTK.Vector3 ToOpenTK()
        {
            return new OpenTK.Vector3(X, Y, Z);
        }
    }
}
