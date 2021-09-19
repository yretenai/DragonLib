﻿using System.Linq;

namespace DragonLib.Numerics
{
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(params float[] values)
        {
            X = values.ElementAtOrDefault(0);
            Y = values.ElementAtOrDefault(1);
        }

        public System.Numerics.Vector2 ToNumerics() => new(X, Y);
        public float[] ToArray() => new[] { X, Y };
    }
}
