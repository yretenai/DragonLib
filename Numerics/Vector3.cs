using System.Linq;

namespace DragonLib.Numerics {
    public struct Vector3 {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(params float[] values) {
            X = values.ElementAtOrDefault(0);
            Y = values.ElementAtOrDefault(1);
            Z = values.ElementAtOrDefault(2);
        }

        public System.Numerics.Vector3 ToNumerics() {
            return new System.Numerics.Vector3(X, Y, Z);
        }

        public float[] ToArray() {
            return new[] { X, Y, Z };
        }
    }
}
