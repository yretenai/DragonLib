using System.Linq;
using System.Numerics;

namespace DragonLib
{
    public struct BBox
    {
        public Vector TopLeft { get; set; }
        public Vector BottomRight { get; set; }

        public BBox(params float[] values)
        {
            TopLeft = new Vector(values.Take(3).ToArray());
            BottomRight = new Vector(values.Take(3).ToArray());
        }

        public BBox(params Vector[] values)
        {
            TopLeft = values.ElementAtOrDefault(0);
            BottomRight = values.ElementAtOrDefault(0);
        }

        public (Vector topLeft, Vector bottomRight) ToTuple() => (TopLeft, BottomRight);

        public (Vector<float> topLeft, Vector<float> bottomRight) ToNumerics() => (TopLeft.ToNumerics(), BottomRight.ToNumerics());
    }
}
