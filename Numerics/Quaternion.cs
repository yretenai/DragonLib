using System;
using System.Linq;

namespace DragonLib.Numerics
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

        public OpenTK.Quaternion ToOpenTK()
        {
            return new OpenTK.Quaternion(X, Y, Z, W);
        }

        // https://github.com/erich666/GraphicsGems/blob/master/gemsiv/euler_angle/EulerAngles.c
        public Vector3 EulerAngles()
        {
            var M = new double[4, 4];
            float Nq = X * X + Y * Y + Z * Z + W * W;
            float s = Nq > 0.0f ? 2.0f / Nq : 0.0f;
            float xs = X * s, ys = Y * s, zs = Z * s;
            float wx = W * xs, wy = W * ys, wz = W * zs;
            float xx = X * xs, xy = X * ys, xz = X * zs;
            float yy = Y * ys, yz = Y * zs, zz = Z * zs;
            M[0, 0] = 1.0f - (yy + zz);
            M[0, 1] = xy - wz;
            M[0, 2] = xz + wy;
            M[1, 0] = xy + wz;
            M[1, 1] = 1.0f - (xx + zz);
            M[1, 2] = yz - wx;
            M[2, 0] = xz - wy;
            M[2, 1] = yz + wx;
            M[2, 2] = 1.0f - (xx + yy);
            M[3, 0] = M[3, 1] = M[3, 2] = M[0, 3] = M[1, 3] = M[2, 3] = 0.0f;
            M[3, 3] = 1.0f;

            var cy = Math.Sqrt(M[0, 0] * M[0, 0] + M[1, 0] * M[1, 0]);

            if (cy > 16 * Double.Epsilon)
                return new Vector3
                {
                    X = (float) Math.Atan2(M[2, 1], M[2, 2]),
                    Y = (float) Math.Atan2(0.0 - M[2, 0], cy),
                    Z = (float) Math.Atan2(M[1, 0], M[0, 0])
                };

            return new Vector3
            {
                X = (float) Math.Atan2(0.0 - M[1, 2], M[1, 1]),
                Y = (float) Math.Atan2(0.0 - M[2, 0], cy),
                Z = 0f
            };
        }
    }
}
