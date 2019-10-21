using System;
using System.Text;

namespace DragonLib
{
    public static class Extensions
    {
        public static string ReadString(this Span<byte> data, Encoding encoding = null)
        {
            var index = data.IndexOf<byte>(0);
            if (index < -1) index = data.Length;

            return (encoding ?? Encoding.UTF8).GetString(data.Slice(0, index));
        }

        public static int Align(this int value, int n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value + (n - value % n);
        }

        public static uint Align(this uint value, uint n)
        {
            if (value < n) return n;
            if (value % n == 0) return value;

            return value + (n - value % n);
        }
    }
}
