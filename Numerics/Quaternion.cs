using System;
using System.Linq;
using JetBrains.Annotations;

namespace DragonLib.Numerics
{
    [PublicAPI]
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

        public OpenTK.Quaternion ToOpenTK() => new OpenTK.Quaternion(X, Y, Z, W);
        public System.Numerics.Quaternion ToNumerics() => new System.Numerics.Quaternion(X, Y, Z, W);
        public float[] ToArray() => new[] { X, Y, Z, W };

        // https://github.com/erich666/GraphicsGems/blob/master/gemsiv/euler_angle/EulerAngles.c
        public Vector3 EulerAngles()
        {
            var m = new double[4, 4];
            var nq = X * X + Y * Y + Z * Z + W * W;
            var s = nq > 0.0f ? 2.0f / nq : 0.0f;
            float xs = X * s, ys = Y * s, zs = Z * s;
            float wx = W * xs, wy = W * ys, wz = W * zs;
            float xx = X * xs, xy = X * ys, xz = X * zs;
            float yy = Y * ys, yz = Y * zs, zz = Z * zs;
            m[0, 0] = 1.0f - (yy + zz);
            m[0, 1] = xy - wz;
            m[0, 2] = xz + wy;
            m[1, 0] = xy + wz;
            m[1, 1] = 1.0f - (xx + zz);
            m[1, 2] = yz - wx;
            m[2, 0] = xz - wy;
            m[2, 1] = yz + wx;
            m[2, 2] = 1.0f - (xx + yy);
            m[3, 0] = m[3, 1] = m[3, 2] = m[0, 3] = m[1, 3] = m[2, 3] = 0.0f;
            m[3, 3] = 1.0f;

            var cy = Math.Sqrt(m[0, 0] * m[0, 0] + m[1, 0] * m[1, 0]);

            if (cy > 16 * Double.Epsilon)
                return new Vector3
                {
                    X = (float) Math.Atan2(m[2, 1], m[2, 2]),
                    Y = (float) Math.Atan2(0.0 - m[2, 0], cy),
                    Z = (float) Math.Atan2(m[1, 0], m[0, 0])
                };

            return new Vector3
            {
                X = (float) Math.Atan2(0.0 - m[1, 2], m[1, 1]),
                Y = (float) Math.Atan2(0.0 - m[2, 0], cy),
                Z = 0f
            };
        }
    }
}
