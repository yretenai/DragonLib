using System.Linq;
using JetBrains.Annotations;

namespace DragonLib.Numerics
{
    [PublicAPI]
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(params float[] values)
        {
            X = values.ElementAtOrDefault(0);
            Y = values.ElementAtOrDefault(1);
        }

        public System.Numerics.Vector2 ToNumerics() => new System.Numerics.Vector2(X, Y);
        public float[] ToArray() => new[] { X, Y };
    }
}
